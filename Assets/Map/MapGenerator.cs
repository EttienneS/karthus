using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class MapGenerator
{
    public MapPreset MapPreset;

    public enum RoadSize
    {
        Single, Double, Triple
    }

    public int Clamp(int value, int min, int max)
    {
        if (value > max)
            return max;

        if (value < min)
            return min;

        return value;
    }

    public Cell CreateCell(int x, int y)
    {
        var cell = new Cell
        {
            X = x,
            Y = y
        };

        Game.MapGrid.Cells.Add(cell);

        Game.MapGrid.AddCellLabel(cell);

        return cell;
    }

    public void FinishBuildings(List<List<Cell>> buildings)
    {
        foreach (var building in buildings)
        {
            var v = Random.value;

            if (!Grow(building, 1).Any(c => c.Structure?.Name == "Road"))
            {
                foreach (var cell in building)
                {
                    if (cell.Structure != null)
                    {
                        Game.StructureController.DestroyStructure(cell.Structure);
                    }
                }
            }
            else
            {
                foreach (var cell in Game.MapGrid.HollowSquare(building))
                {
                    cell.CreateStructure("Wood Tile");
                }

                var doors = GetPossibleDoors(building);
                if (doors.Count > 0)
                {
                    doors[Random.Range(0, doors.Count)].CreateStructure("Wood Tile");
                }
                else
                {
                    foreach (var cell in building)
                    {
                        if (cell.Structure != null)
                        {
                            Game.StructureController.DestroyStructure(cell.Structure);
                        }
                    }
                }
            }
        }
    }

    public List<Cell> GetCorners(List<Cell> square)
    {
        var minMax = Game.MapGrid.GetMinMax(square);

        return new List<Cell>
            {
                Game.MapGrid.GetCellAtCoordinate(minMax.minx, minMax.miny),
                Game.MapGrid.GetCellAtCoordinate(minMax.minx, minMax.maxy),
                Game.MapGrid.GetCellAtCoordinate(minMax.maxx, minMax.miny),
                Game.MapGrid.GetCellAtCoordinate(minMax.maxx, minMax.maxy)
            };
    }

    public List<Cell> GetPossibleDoors(List<Cell> building)
    {
        var doors = new List<Cell>();
        var corners = GetCorners(building);
        foreach (var cell in Game.MapGrid.GetBorder(building))
        {
            if (corners.Any(c => c.X == cell.X && c.Y == cell.Y))
            {
                continue;
            }

            var neighbours = cell.Neighbors.Count(c => c?.Structure?.Name == "Road");
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

                    if (tx >= 0 && tx < Game.MapGrid.Width && ty >= 0 && ty < Game.MapGrid.Height)
                    {
                        group.Add(Game.MapGrid.GetCellAtCoordinate(tx, ty));
                    }
                }
            }
        }

        return group.Distinct().ToList();
    }

    public void MakeRoad(Cell point1, Cell point2, RoadSize roadSize = RoadSize.Double)
    {
        var path = Pathfinder.FindPath(point1.GetRandomNeighbor(),
                                                 point2.GetRandomNeighbor(),
                                                 Mobility.AbyssWalk);

        if (path == null || path.Count == 0)
        {
            return;
        }

        foreach (var cell in path)
        {
            Direction[] dirs;
            switch (roadSize)
            {
                case RoadSize.Single:
                    dirs = new Direction[] { };
                    break;

                case RoadSize.Double:
                    dirs = new Direction[] { Direction.N, Direction.W };
                    break;

                case RoadSize.Triple:
                    dirs = new Direction[] { Direction.N, Direction.W, Direction.E, Direction.S };
                    break;

                default:
                    throw new Exception("Unknown road type");
            }

            foreach (var dir in dirs)
            {
                try
                {
                    var neighbour = cell.GetNeighbor(dir);
                    if (neighbour == null || neighbour.TravelCost < 0 || neighbour.Structure != null)
                    {
                        var tempDir = Direction.E;
                        if (dir == Direction.N)
                        {
                            tempDir = Direction.S;
                        }

                        neighbour = cell.GetNeighbor(tempDir);
                    }

                    if (neighbour == null || neighbour.TravelCost < 0 || neighbour.Structure != null)
                    {
                        continue;
                    }

                    neighbour.SetStructure(Game.StructureController.GetStructure("Road", FactionController.WorldFaction));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Road fail: {ex}");
                }
            }

            if (cell != null && cell.TravelCost > 0 && cell.Structure == null)
            {
                cell.SetStructure(Game.StructureController.GetStructure("Road", FactionController.WorldFaction));
            }
        }
    }

    public Structure MakeRune(Cell location, string name, Faction faction)
    {
        Game.MapGrid.BindCell(location, faction.Core);

        if (location.Structure != null)
            Game.StructureController.DestroyStructure(location.Structure);

        var rune = location.CreateStructure(name, faction.FactionName);
        location.SetStructure(rune);

        if (rune.Spell != null)
        {
            Game.MagicController.AddRune(rune);
        }

        return rune;
    }

    public void SpawnCreatures()
    {
        MakeFactionCore(Game.MapGrid.Center, FactionController.PlayerFaction);

        for (int i = 0; i < 3; i++)
        {
            Game.CreatureController.SpawnCreature(Game.CreatureController.GetCreatureOfType("Person"),
                                         Game.MapGrid.Center.GetNeighbor(Helpers.RandomEnumValue<Direction>()),
                                         FactionController.PlayerFaction);
        }

        Game.CameraController.MoveToCell(Game.MapGrid.Center.GetNeighbor(Direction.E));
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

        var streets = CreateStreets(Game.MapGrid.Center, 0.5f);
        var buildings = new List<List<Cell>>();
        foreach (var street in streets)
        {
            buildings.AddRange(CreateBuildings(7, 7, 3, 3, street));
        }
        FinishBuildings(buildings);
    }

    internal void GenerateMapFromPreset()
    {
        Game.MapGrid.Cells = new List<Cell>();

        for (var y = 0; y < Game.MapGrid.Height; y++)
        {
            for (var x = 0; x < Game.MapGrid.Width; x++)
            {
                CreateCell(x, y);
            }
        }

        LinkNeighbours();

        if (Game.MapGrid.Seed == 0)
        {
            Game.MapGrid.Seed = Random.Range(1, 10000);
        }

        GenerateMapCells();

        ResetSearchPriorities();
    }

    internal void LinkNeighbours()
    {
        for (var y = 0; y < Game.MapGrid.Height; y++)
        {
            for (var x = 0; x < Game.MapGrid.Width; x++)
            {
                var cell = Game.MapGrid.CellLookup[(x, y)];

                if (x > 0)
                {
                    cell.SetNeighbor(Direction.W, Game.MapGrid.CellLookup[(x - 1, y)]);

                    if (y > 0)
                    {
                        cell.SetNeighbor(Direction.SW, Game.MapGrid.CellLookup[(x - 1, y - 1)]);

                        if (x < Game.MapGrid.Width - 1)
                        {
                            cell.SetNeighbor(Direction.SE, Game.MapGrid.CellLookup[(x + 1, y - 1)]);
                        }
                    }
                }

                if (y > 0)
                {
                    cell.SetNeighbor(Direction.S, Game.MapGrid.CellLookup[(x, y - 1)]);
                }
            }
        }
    }

    internal void Make()
    {
        var sw = new Stopwatch();

        sw.Start();

        MapPreset = new MapPreset((0.80f, CellType.Mountain),
                                  (0.7f, CellType.Stone),
                                  (0.5f, CellType.Forest),
                                  (0.3f, CellType.Grass),
                                  (0.2f, CellType.Dirt),
                                  (0.0f, CellType.Water));

        GenerateMapFromPreset();
        Debug.Log($"Generated map in {sw.Elapsed}");
        sw.Restart();

        CreateTown();
        Debug.Log($"Generated towns in {sw.Elapsed}");
        sw.Restart();

        CreateLeyLines();
        Debug.Log($"Created ley lines in {sw.Elapsed}");
        sw.Restart();

        SpawnCreatures();
        Debug.Log($"Spawned creatures in {sw.Elapsed}");
        sw.Restart();

        SpawnMonsters();
        Debug.Log($"Spawned monsters in {sw.Elapsed}");
        sw.Restart();
    }

    internal void PopulateCell(Cell cell)
    {
        if (cell.Structure?.Name == "Reserved")
        {
            Game.StructureController.DestroyStructure(cell.Structure);
        }

        if (cell.Structure != null)
        {
            return;
        }

        var value = Random.value;
        var world = FactionController.Factions[FactionConstants.World];
        switch (cell.CellType)
        {
            case CellType.Grass:
                if (value > 0.8)
                {
                    cell.SetStructure(Game.StructureController.GetStructure("Bush", world));
                }
                break;

            case CellType.Forest:
                if (value > 0.95)
                {
                    cell.SetStructure(Game.StructureController.GetStructure("Tree", world));
                }
                else if (value > 0.8)
                {
                    cell.SetStructure(Game.StructureController.GetStructure("Bush", world));
                }
                break;
        }
    }

    internal void ResetSearchPriorities()
    {
        // ensure that all cells have their phases reset
        for (var y = 0; y < Game.MapGrid.Height; y++)
        {
            for (var x = 0; x < Game.MapGrid.Width; x++)
            {
                Game.MapGrid.CellLookup[(x, y)].SearchPhase = 0;
            }
        }
    }

    private static void SpawnMonsters()
    {
        for (int i = 0; i < Game.MapGrid.Width / 10; i++)
        {
            Game.CreatureController.SpawnCreature(Game.CreatureController.GetCreatureOfType("AbyssWraith"),
                                             Game.MapGrid.GetRandomCell(),
                                             FactionController.MonsterFaction);
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
                        Game.MapGrid.GetCellAtCoordinate(cell.X + 1, cell.Y),
                        Game.MapGrid.GetCellAtCoordinate(cell.X - 1, cell.Y),
                        Game.MapGrid.GetCellAtCoordinate(cell.X, cell.Y + 1),
                        Game.MapGrid.GetCellAtCoordinate(cell.X, cell.Y - 1),
                    };
            foreach (var neighbour in neighbours)
            {
                bool found = false;
                for (int width = -maxWidth; width < maxWidth; width++)
                {
                    for (int height = -maxHeight; height < maxHeight; height++)
                    {
                        var structure = Game.MapGrid.GetRectangle(neighbour, width, height);
                        var measure = Game.MapGrid.GetWidthAndHeight(structure);

                        if (measure.Item1 < minWidth)
                        {
                            continue;
                        }

                        if (measure.Item2 < minHeight)
                        {
                            continue;
                        }

                        if (structure.TrueForAll(c => c.Structure == null))
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

                foreach (var buffer in Grow(biggest, Random.Range(1, 3)))
                {
                    if (buffer.Structure == null)
                    {
                        buffer.CreateStructure("Reserved");
                    }
                }
            }
        }

        return buildings;
    }

    private void CreateLeyLines()
    {
        var nexusPoints = new List<Cell>();
        for (int i = 0; i < Game.MapGrid.Width / 10; i++)
        {
            var point = Game.MapGrid.GetRandomCell();
            nexusPoints.Add(point);
            MakeRune(point, "LeySpring", FactionController.WorldFaction);
        }

        var v = Enum.GetValues(typeof(ManaColor));
        var counter = 0;
        foreach (var cell in nexusPoints)
        {
            var target = nexusPoints[(int)(Random.value * (nexusPoints.Count - 1))];
            Game.LeyLineController.MakeLine(Game.MapGrid.GetLine(cell, target), (ManaColor)v.GetValue(counter));

            counter++;

            if (counter >= v.Length)
            {
                counter = 0;
            }
        }
    }

    private List<List<Cell>> CreateStreets(Cell center, float momentum)
    {
        var streets = new List<List<Cell>>();
        var mainStreet = Game.MapGrid.GetDiameterLine(center, Random.Range(30, 90), Random.Range(-10, 10));
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

    private void GenerateMapCells()
    {
        for (int x = 0; x < Game.MapGrid.Width; x++)
        {
            for (int y = 0; y < Game.MapGrid.Height; y++)
            {
                var cell = Game.MapGrid.GetCellAtCoordinate(x, y);

                cell.Height = MapPreset.GetCellHeight(cell.X, cell.Y);
            }
        }
    }

    private void MakeFactionCore(Cell center, Faction faction)
    {
        Game.MapGrid.Center.SetStructure(FactionController.PlayerFaction.Core);
        foreach (var cell in Game.MapGrid.GetCircle(center, 25))
        {
            cell.Binding = faction.Core;
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

        var street = Game.MapGrid.GetLine(crossingPoint, Game.MapGrid.GetPointAtDistanceOnAngle(crossingPoint, length, angle));

        foreach (var cell in street)
        {
            cell.CreateStructure("Road");
        }

        streets.Add(street);
        momentum *= Random.value + 1f;
        length = (int)((length * Random.value) + 1f);

        if (momentum > 0.1f)
        {
            for (int i = (int)Math.Ceiling(length / 3f); i < street.Count; i++)
            {
                if (Random.value > 0.9)
                {
                    MakeStreet(street[i], length, !vertical, momentum, color, streets);
                    i += 5;
                }
            }
        }
    }

}