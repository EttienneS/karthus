using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class MapGenerator
{
    public Biome Biome;

    public bool Busy;
    public bool Done;
    public string Status;
    private List<Biome> _biomeTemplates;

    public List<Biome> BiomeTemplates
    {
        get
        {
            if (_biomeTemplates == null)
            {
                _biomeTemplates = new List<Biome>();
                foreach (var biomeFile in Game.FileController.BiomeFiles)
                {
                    _biomeTemplates.Add(biomeFile.text.LoadJson<Biome>());
                }
            }

            return _biomeTemplates;
        }
    }

    public Cell CreateCell(int x, int y)
    {
        var cell = new Cell
        {
            X = x,
            Y = y
        };

        Game.Map.Cells.Add(cell);

        return cell;
    }

    public void FinishBuildings(List<List<Cell>> buildings)
    {
        foreach (var building in buildings)
        {
            var v = Random.value;

            if (!Grow(building, 1).Any(c => c.Floor?.Name == "Road"))
            {
                foreach (var cell in building)
                {
                    cell.Clear();
                }
            }
            else
            {
                foreach (var cell in Game.Map.HollowSquare(building))
                {
                    Game.StructureController.DestroyStructure(cell.Structure);
                    cell.CreateStructure("Wood Tile");
                }

                var doors = GetPossibleDoors(building);
                if (doors.Count > 0)
                {
                    var door = doors[Random.Range(0, doors.Count)];
                    Game.StructureController.DestroyStructure(door.Structure);
                    door.CreateStructure("Wood Tile");
                }
                else
                {
                    foreach (var cell in building)
                    {
                        cell.Clear();
                    }
                }
            }
        }
    }

    public List<Cell> GetCorners(List<Cell> square)
    {
        var minMax = Game.Map.GetMinMax(square);

        return new List<Cell>
            {
                Game.Map.GetCellAtCoordinate(minMax.minx, minMax.miny),
                Game.Map.GetCellAtCoordinate(minMax.minx, minMax.maxy),
                Game.Map.GetCellAtCoordinate(minMax.maxx, minMax.miny),
                Game.Map.GetCellAtCoordinate(minMax.maxx, minMax.maxy)
            };
    }

    public List<Cell> GetPossibleDoors(List<Cell> building)
    {
        var doors = new List<Cell>();
        var corners = GetCorners(building);
        foreach (var cell in Game.Map.GetBorder(building))
        {
            if (corners.Any(c => c.X == cell.X && c.Y == cell.Y))
            {
                continue;
            }

            var neighbours = cell.Neighbors.Count(c => c?.Floor?.Name == "Road");
            if (neighbours > 1)
            {
                doors.Add(cell);
            }
        }

        return doors;
    }

    public Biome GetRandomBiome()
    {
        return BiomeTemplates.Where(t => t.Name != "Default").GetRandomItem().CloneJson();
    }

    public List<Cell> Grow(List<Cell> cells, int size)
    {
        var group = cells.ToList();

        foreach (var cell in cells)
        {
            for (int x = -size; x <= size; x++)
            {
                for (int y = -size; y <= size; y++)
                {
                    var tx = cell.X + x;
                    var ty = cell.Y + y;

                    if (group.Any(g => g.X == tx && g.Y == ty))
                    {
                        continue;
                    }

                    if (tx >= 0 && tx < Game.Map.Width && ty >= 0 && ty < Game.Map.Height)
                    {
                        group.Add(Game.Map.GetCellAtCoordinate(tx, ty));
                    }
                }
            }
        }

        return group.Distinct().ToList();
    }

    public IEnumerator Work()
    {
        Biome = BiomeTemplates.First(b => b.Name == "Default");

        Game.Map.Cells = new List<Cell>();
        Game.SetLoadStatus("Create cells", 0.08f);
        for (var y = 0; y < Game.Map.Height; y++)
        {
            for (var x = 0; x < Game.Map.Width; x++)
            {
                CreateCell(x, y);
            }
        }
        yield return null;

        Game.SetLoadStatus("Link cells", 0.12f);
        LinkNeighbours();
        yield return null;

        Game.SetLoadStatus("Set cell heights", 0.15f);
        SetInitialCellHeights();
        yield return null;

        Game.SetLoadStatus("Reset search priorities", 0.20f);
        ResetSearchPriorities();
        yield return null;

        Game.SetLoadStatus("Bootstrap factions", 0.35f);
        MakeFactionBootStrap(Game.FactionController.PlayerFaction);
        yield return null;

        Game.SetLoadStatus("Spawn creatures", 0.40f);
        SpawnCreatures();
        yield return null;

        Game.SetLoadStatus("Build render chunks", 0.45f);
        var chunks = GetRenderChunks(100);
        yield return null;

        Game.SetLoadStatus("Render chunks", 0.5f);
        var i = 0f;
        var totalProgress = 0.3f;
        var totalChunks = chunks.Count;
        foreach (var chunk in chunks)
        {
            i++;
            Game.SetLoadStatus($"Draw {i + 1}/{totalChunks}", 0.5f + i / totalChunks * totalProgress);

            DrawChunk(chunk);
            yield return null;
        }

        Done = true;
    }

    internal void CreateTown()
    {
        // approach
        // City centers: Pick some points of the still empty map as main traffic nodes. They should be evenly distributed around the map
        // Highways: Connect the main traffic nodes to their neighbors and to the outside world using major roads.
        // Freeways: Subdivide the cells generated by the major roads by creating some minor roads.
        // Streets: Repeat the subdivision process recursively with smaller and smaller roads until you've reached the desired building block size
        // Blocks: Decide the purpose of each building block(residential, retail, corporate, industrial..).Relevant factors are the sizes of the neighboring roads and the distance from the center.
        // Allotments: Divide the edges of all building blocks into lots(this means each lot has at least one edge which is connected to a road).
        // Buildings: Generate a fitting building for each lot.

        var streets = CreateStreets(Game.Map.Center, 0.5f);
        var buildings = new List<List<Cell>>();
        foreach (var street in streets)
        {
            buildings.AddRange(CreateBuildings(7, 7, 3, 3, street));
        }
        FinishBuildings(buildings);
    }

    internal void GenerateBaseMap()
    {
    }

    internal void LinkNeighbours()
    {
        for (var y = 0; y < Game.Map.Height; y++)
        {
            for (var x = 0; x < Game.Map.Width; x++)
            {
                var cell = Game.Map.CellLookup[(x, y)];

                if (x > 0)
                {
                    cell.SetNeighbor(Direction.W, Game.Map.CellLookup[(x - 1, y)]);

                    if (y > 0)
                    {
                        cell.SetNeighbor(Direction.SW, Game.Map.CellLookup[(x - 1, y - 1)]);

                        if (x < Game.Map.Width - 1)
                        {
                            cell.SetNeighbor(Direction.SE, Game.Map.CellLookup[(x + 1, y - 1)]);
                        }
                    }
                }

                if (y > 0)
                {
                    cell.SetNeighbor(Direction.S, Game.Map.CellLookup[(x, y - 1)]);
                }
            }
        }
    }

    internal void ResetSearchPriorities()
    {
        // ensure that all cells have their phases reset
        for (var y = 0; y < Game.Map.Height; y++)
        {
            for (var x = 0; x < Game.Map.Width; x++)
            {
                Game.Map.CellLookup[(x, y)].SearchPhase = 0;
            }
        }
    }

    private static void DrawChunk(List<Cell> chunk)
    {
        chunk.ForEach(c => c.Populate());
        var tiles = chunk.Select(c => c.Tile).ToArray();
        var coords = chunk.Select(c => c.ToVector3Int()).ToArray();
        Game.Map.Tilemap.SetTiles(coords, tiles);
        Game.StructureController.DrawAllStructures(chunk);
    }

    private static List<List<Cell>> GetRenderChunks(int chunkSize)
    {
        var chunks = new List<List<Cell>>();

        var wchunks = Game.Map.Width / chunkSize;
        var hchunks = Game.Map.Height / chunkSize;

        for (int w = 0; w < wchunks; w++)
        {
            for (int h = 0; h < hchunks; h++)
            {
                var chunk = new List<Cell>();

                for (int x = 0; x < chunkSize; x++)
                {
                    for (int y = 0; y < chunkSize; y++)
                    {
                        chunk.Add(Game.Map.GetCellAtCoordinate(x + (w * chunkSize), y + (h * chunkSize)));
                    }
                }

                chunks.Add(chunk);
            }
        }
        return chunks;
    }

    private static void SpawnCreatures()
    {
        foreach (var monster in Game.CreatureController.Beastiary)
        {
            if (monster.Key == "Person")
            {
                continue;
            }

            for (int i = 0; i < Game.Map.Width / 50; i++)
            {
                var creature = Game.CreatureController.GetCreatureOfType(monster.Key);

                var spot = Game.Map.GetRandomCell();
                if (spot.TravelCost <= 0 && creature.Mobility != Mobility.Fly)
                {
                    spot = Game.Map.Cells.Where(c => c.TravelCost > 0).GetRandomItem();
                }
                Game.CreatureController.SpawnCreature(creature, spot, Game.FactionController.MonsterFaction);
            }
        }
    }

    private List<List<Cell>> CreateBuildings(int maxWidth, int maxHeight, int minWidth, int minHeight, List<Cell> street)
    {
        var buildings = new List<List<Cell>>();
        for (int cellIndex = 0; cellIndex < street.Count - 1; cellIndex++)
        {
            var cell = street[cellIndex];
            var biggest = new List<Cell>();

            var neighbours = new List<Cell>
                    {
                        cell,
                        Game.Map.GetCellAtCoordinate(cell.X + 1, cell.Y),
                        Game.Map.GetCellAtCoordinate(cell.X - 1, cell.Y),
                        Game.Map.GetCellAtCoordinate(cell.X, cell.Y + 1),
                        Game.Map.GetCellAtCoordinate(cell.X, cell.Y - 1),
                    };
            foreach (var neighbour in neighbours)
            {
                bool found = false;
                for (int width = -maxWidth; width < maxWidth; width++)
                {
                    for (int height = -maxHeight; height < maxHeight; height++)
                    {
                        var structure = Game.Map.GetRectangle(neighbour, width, height);
                        var measure = Game.Map.GetWidthAndHeight(structure);

                        if (measure.Item1 < minWidth)
                        {
                            continue;
                        }

                        if (measure.Item2 < minHeight)
                        {
                            continue;
                        }

                        if (structure.TrueForAll(c => c.Empty()))
                        {
                            if (Random.value > 0.9)
                            {
                                biggest = structure;
                                found = true;
                                break;
                            }

                            if (structure.Count > biggest.Count)
                            {
                                biggest = structure;
                                if (biggest.Count >= maxWidth * maxHeight)
                                {
                                    // max size for space found stop searching
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (found)
                    {
                        break;
                    }
                }

                if (found)
                {
                    break;
                }
            }

            if (biggest.Count > 0)
            {
                var col = Random.Range(1, 15);
                biggest.ForEach(c => c.CreateStructure("Stone Wall"));
                buildings.Add(biggest);

                var avgHeight = biggest.Average(c => c.Height);
                foreach (var buffer in Grow(biggest, Random.Range(1, 3)))
                {
                    buffer.Height = avgHeight;
                    if (buffer.Structure == null && buffer.Floor == null)
                    {
                        buffer.CreateStructure("Reserved");
                    }
                }
            }
        }

        return buildings;
    }

    private List<List<Cell>> CreateStreets(Cell center, float momentum)
    {
        var streets = new List<List<Cell>>();
        var mainStreet = Game.Map.GetDiameterLine(center, Random.Range(30, 90), Random.Range(-10, 10));
        mainStreet.ForEach(c => c.CreateStructure("Road"));
        streets.Add(mainStreet);

        for (int i = 1; i < mainStreet.Count; i++)
        {
            if (Random.value > 0.8)
            {
                MakeStreet(mainStreet[i], Random.Range(15, 25), true, momentum, 6, streets);
                i += 5;
            }
        }

        return streets;
    }

    private void MakeFactionBootStrap(Faction faction)
    {
        var center = Game.Map.GetNearestPathableCell(Game.Map.Center, Mobility.Walk, 25);
        var core = center.CreateStructure("Battery", faction.FactionName);
        core.ManaValue.GainMana(ManaColor.Green, 20);
        core.ManaValue.GainMana(ManaColor.Red, 20);
        core.ManaValue.GainMana(ManaColor.Blue, 20);
        core.ManaValue.GainMana(ManaColor.White, 20);
        core.ManaValue.GainMana(ManaColor.Black, 20);

        for (int i = 0; i < 3; i++)
        {
            Game.CreatureController.SpawnCreature(Game.CreatureController.GetCreatureOfType("Person"),
                                                  Game.Map.GetNearestPathableCell(center, Mobility.Walk, 25),
                                                  Game.FactionController.PlayerFaction);
        }
    }

    private void MakeStreet(Cell crossingPoint, int length, bool vertical, double momentum, int color, List<List<Cell>> streets)
    {
        var degrees = vertical ? new[] { 90, 270 } : new[] { 0, 180 };
        var angle = degrees[Random.Range(0, 2)];

        if (Random.value > 0.7)
        {
            angle += Random.Range(-10, 10);
        }

        var street = Game.Map.GetLine(crossingPoint, Game.Map.GetPointAtDistanceOnAngle(crossingPoint, length, angle));

        foreach (var cell in street)
        {
            cell.CreateStructure("Road");
        }

        streets.Add(street);
        momentum *= Random.value + 1f;
        length = (int)((length * Random.value) + 1f);

        if (momentum > 0.1f)
        {
            for (int i = (int)Math.Ceiling(length / 2f); i < street.Count; i++)
            {
                if (Random.value > 0.95)
                {
                    MakeStreet(street[i], length, !vertical, momentum, color, streets);
                    i += Random.Range(5, 10);
                }
            }
        }
    }

    private void SetInitialCellHeights()
    {
        for (int x = 0; x < Game.Map.Width; x++)
        {
            for (int y = 0; y < Game.Map.Height; y++)
            {
                var cell = Game.Map.GetCellAtCoordinate(x, y);
                cell.Height = Game.Map.GetCellHeight(cell.X, cell.Y);
            }
        }
    }
}