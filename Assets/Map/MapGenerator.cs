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

    public (Cell bottomLeft, Cell bottomRight, Cell topLeft, Cell topRight) GetCorners(List<Cell> square)
    {
        var minMax = Game.Map.GetMinMax(square);

        return (Game.Map.GetCellAtCoordinate(minMax.minx, minMax.miny),
                Game.Map.GetCellAtCoordinate(minMax.maxx, minMax.miny),
                Game.Map.GetCellAtCoordinate(minMax.minx, minMax.maxy),
                Game.Map.GetCellAtCoordinate(minMax.maxx, minMax.maxy));
    }

    public List<Cell> GetPossibleDoors(List<Cell> building)
    {
        var doors = new List<Cell>();
        foreach (var cell in Game.Map.GetBorder(building))
        {
            var neighbours = cell.Neighbors.Count(c => c?.Floor?.Name == "Road");
            if (neighbours > 1)
            {
                doors.Add(cell);
            }
        }

        return doors;
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

                    if (tx >= Game.Map.MinX && tx < Game.Map.MaxX && ty >= Game.Map.MinY && ty < Game.Map.MaxY)
                    {
                        group.Add(Game.Map.GetCellAtCoordinate(tx, ty));
                    }
                }
            }
        }

        return group.Distinct().ToList();
    }

    public void MakeFactionBootStrap(Faction faction)
    {
        var center = Game.Map.GetNearestPathableCell(Game.Map.Center, Mobility.Walk, 25);

        Game.FactionController.PlayerFaction.HomeCells.AddRange(Game.Map.GetCircle(Game.Map.Center, 15));

        var open = Game.Map.GetCircle(center, 8).Where(c => c.Pathable(Mobility.Walk));
        Game.ItemController.SpawnItem("Berries", open.GetRandomItem(), 100);
        Game.ItemController.SpawnItem("Wood", open.GetRandomItem(), 25);
        Game.ItemController.SpawnItem("Stone", open.GetRandomItem(), 25);

        for (int i = 0; i < 3; i++)
        {
            var c = Game.CreatureController.SpawnCreature(Game.CreatureController.GetCreatureOfType("Person"),
                                                     Game.Map.GetNearestPathableCell(center, Mobility.Walk, 25),
                                                     faction);
        }
    }

    public void SpawnCreatures()
    {
        foreach (var monster in Game.CreatureController.Beastiary)
        {
            if (monster.Key == "Person")
            {
                continue;
            }

            for (int i = Game.Map.MinY; i < Game.Map.MaxX / 50; i++)
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

    public IEnumerator Work()
    {
        Biome = BiomeTemplates.First(b => b.Name == "Default");

        Game.Map.Cells = new List<Cell>();
        Game.Instance.SetLoadStatus("Create cells", 0.08f);
        MakeCells(Game.Map.MinX, Game.Map.MinY, Game.Map.MaxX, Game.Map.MaxY);
        Game.Instance.SetLoadStatus("Create cells", 0.18f);
        yield return null;

        Game.Instance.SetLoadStatus("Build render chunks", 0.45f);
        var chunks = GetRenderChunks(Game.Map.ChunkSize);
        Game.Instance.SetLoadStatus("Build render chunks", 0.45f);
        yield return null;

        Game.Instance.SetLoadStatus("Render chunks", 0.5f);
        var i = 0f;
        var totalProgress = 0.3f;
        var totalChunks = chunks.Count;
        foreach (var chunk in chunks)
        {
            i++;
            Game.Instance.SetLoadStatus($"Draw {i}/{totalChunks}", 0.5f + (i / totalChunks * totalProgress));
            DrawChunk(chunk);
            yield return null;
        }

        Done = true;
    }

    internal void CreateTown()
    {
        var streets = CreateStreets(Game.Map.Center, 0.5f);
        var buildings = new List<List<Cell>>();
        foreach (var street in streets)
        {
            buildings.AddRange(CreateBuildings(7, 7, 3, 3, street));
        }
        FinishBuildings(buildings);
    }

    internal void MakeCells(int miny, int minx, int maxy, int maxx)
    {
        for (var y = miny; y < maxy; y++)
        {
            for (var x = minx; x < maxx; x++)
            {
                CreateCell(x, y);
            }
        }

        for (var y = miny; y < maxy; y++)
        {
            for (var x = minx; x < maxx; x++)
            {
                var cell = Game.Map.CellLookup[(x, y)];
                cell.SearchPhase = 0;
                if (x > Game.Map.MinX)
                {
                    cell.SetNeighbor(Direction.W, Game.Map.CellLookup[(x - 1, y)]);

                    if (y > Game.Map.MinY)
                    {
                        cell.SetNeighbor(Direction.SW, Game.Map.CellLookup[(x - 1, y - 1)]);

                        if (x < Game.Map.MaxX - 1)
                        {
                            cell.SetNeighbor(Direction.SE, Game.Map.CellLookup[(x + 1, y - 1)]);
                        }
                    }
                }

                if (y > Game.Map.MinY)
                {
                    cell.SetNeighbor(Direction.S, Game.Map.CellLookup[(x, y - 1)]);
                }
            }
        }
    }

    internal void ResetSearchPriorities()
    {
        // ensure that all cells have their phases reset
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
                        var structure = Game.Map.GetRectangle(neighbour.X, neighbour.Y, width, height);
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

    private void DrawChunk(List<Cell> chunk)
    {
        chunk.ForEach(c => c.Populate());
        var tiles = chunk.Select(c => c.Tile).ToArray();
        var coords = chunk.Select(c => c.ToVector3Int()).ToArray();
        Game.Map.Tilemap.SetTiles(coords, tiles);
        Game.StructureController.DrawStructures(chunk);
    }

    private List<List<Cell>> GetRenderChunks(int chunkSize)
    {
        var chunks = new List<List<Cell>>();

        var wchunks = (Game.Map.MaxX - Game.Map.MinX) / chunkSize;
        var hchunks = (Game.Map.MaxY - Game.Map.MinY) / chunkSize;

        for (int w = 0; w < wchunks; w++)
        {
            for (int h = 0; h < hchunks; h++)
            {
                var chunk = new List<Cell>();

                for (int x = Game.Map.MinX; x < chunkSize + Game.Map.MinX; x++)
                {
                    for (int y = Game.Map.MinY; y < chunkSize + Game.Map.MinY; y++)
                    {
                        chunk.Add(Game.Map.GetCellAtCoordinate(x + (w * chunkSize), y + (h * chunkSize)));
                    }
                }

                chunks.Add(chunk);
            }
        }
        return chunks;
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
}