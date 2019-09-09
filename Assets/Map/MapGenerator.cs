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

    public List<List<CellData>> Buildings { get; set; } = new List<List<CellData>>();

    public List<List<CellData>> Streets { get; set; } = new List<List<CellData>>();

    public int Clamp(int value, int min, int max)
    {
        if (value > max)
            return max;

        if (value < min)
            return min;

        return value;
    }

    public CellData CreateCell(int x, int y)
    {
        var cell = new CellData
        {
            X = x,
            Y = y
        };

        Game.MapGrid.Cells.Add(cell);

        Game.MapGrid.AddCellLabel(cell);

        return cell;
    }

    public List<CellData> GetBorder(List<CellData> square)
    {
        var frame = square.ToList();
        var hollow = HollowSquare(square);
        frame.RemoveAll(c => hollow.Contains(c));

        return frame;
    }

    public List<CellData> GetLine(CellData a, CellData b)
    {
        // Bresenham line algorithm [https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm]
        // https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm

        var line = new List<CellData>();

        var x = a.X;
        var y = a.Y;
        var x2 = b.X;
        var y2 = b.Y;

        var w = x2 - x;
        var h = y2 - y;

        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;

        var longest = Math.Abs(w);
        var shortest = Math.Abs(h);
        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        var numerator = longest >> 1;
        for (var i = 0; i <= longest; i++)
        {
            line.Add(Game.MapGrid.GetCellAtCoordinate(x, y));
            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }

        return line;
    }

    public (int minx, int maxx, int miny, int maxy) GetMinMax(List<CellData> cells)
    {
        var minx = int.MaxValue;
        var maxx = int.MinValue;

        var miny = int.MaxValue;
        var maxy = int.MinValue;

        foreach (var cell in cells)
        {
            if (cell.X > maxx)
            {
                maxx = cell.X;
            }
            if (cell.X < minx)
            {
                minx = cell.X;
            }
            if (cell.Y > maxy)
            {
                maxy = cell.Y;
            }
            if (cell.Y < miny)
            {
                miny = cell.Y;
            }
        }

        return (minx, maxx, miny, maxy);
    }

    public List<CellData> GetRectangle(CellData cell, int width, int height)
    {
        var cells = new List<CellData>();

        var fromX = Math.Min(cell.X, cell.X + width);
        var toX = Math.Max(cell.X, cell.X + width);

        var fromY = Math.Min(cell.Y, cell.Y + height);
        var toY = Math.Max(cell.Y, cell.Y + height);

        for (var x = fromX; x < toX; x++)
        {
            for (var y = fromY; y < toY; y++)
            {
                Game.MapGrid.AddCellIfValid(x, y, cells);
            }
        }

        return cells;
    }

    public (int, int) GetWidthAndHeight(List<CellData> cells)
    {
        var minMax = GetMinMax(cells);
        return (minMax.maxx - minMax.minx, minMax.maxy - minMax.miny);
    }

    public List<CellData> HollowSquare(List<CellData> square)
    {
        var minMax = GetMinMax(square);
        var src = Game.MapGrid.GetCellAtCoordinate(minMax.minx + 1, minMax.miny + 1);
        return GetRectangle(src,
                            minMax.maxx - minMax.minx - 1,
                            minMax.maxy - minMax.miny - 1);
    }

    public void MakeRoad(CellData point1, CellData point2, RoadSize roadSize = RoadSize.Double)
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

    public Structure MakeRune(CellData location, string name, Faction faction)
    {
        Game.MapGrid.BindCell(location, faction.Core);

        if (location.Structure != null)
            Game.StructureController.DestroyStructure(location.Structure);

        var rune = Game.StructureController.GetStructure(name, faction);
        Game.MapGrid.BindCell(location, rune);
        location.SetStructure(rune);

        if (name == "BindRune")
        {
            foreach (var c in Game.MapGrid.BleedGroup(Game.MapGrid.GetCircle(location, Random.Range(4, 7))))
            {
                Game.MapGrid.BindCell(c, rune);
            }
        }

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

        CreateStreets();
        CreateBuildings(7, 7, 3, 3);
    }

    internal void GenerateMapFromPreset()
    {
        Game.MapGrid.Cells = new List<CellData>();

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

    internal CellData GetCellAttRadian(CellData center, int radius, int angle)
    {
        var mineX = Clamp((int)(center.X + (radius * Math.Cos(angle))), 0, Game.MapGrid.Width);
        var mineY = Clamp((int)(center.Y + (radius * Math.Sin(angle))), 0, Game.MapGrid.Height);

        return Game.MapGrid.GetCellAtCoordinate(mineX, mineY);
    }

    internal float GetDegreesBetweenPoints(CellData point1, CellData point2)
    {
        var deltaX = point1.X - point2.X;
        var deltaY = point1.Y - point2.Y;

        var radAngle = Math.Atan2(deltaY, deltaX);
        var degreeAngle = radAngle * 180.0 / Math.PI;

        return (float)(180.0 - degreeAngle);
    }

    internal List<CellData> GetDiameterLine(CellData center, int lenght, int angle = 0)
    {
        return GetLine(GetPointAtDistanceOnAngle(center, lenght / 2, angle),
                       GetPointAtDistanceOnAngle(center, lenght / 2, angle + 180));
    }

    internal CellData GetPointAtDistanceOnAngle(CellData origin, int distance, float angle)
    {
        var radians = angle * Math.PI / 180.0;

        // cater for right angle scenarios
        var tX = origin.X;
        var tY = origin.Y;

        if (angle != 0 && angle != 180)
        {
            tY = (int)((Math.Sin(-radians) * distance) + origin.Y);
        }

        if (angle != 90 && angle != 270)
        {
            tX = (int)((Math.Cos(radians) * distance) + origin.X);
        }

        // add 1 to offset rounding errors
        return Game.MapGrid.GetCellAtCoordinate(tX, tY);
    }

    internal CellData GetRandomRadian(CellData origin, int radius)
    {
        var angle = Random.Range(0, 360);
        var mineX = Clamp((int)(origin.X + (radius * Math.Cos(angle))), 0, Game.MapGrid.Width);
        var mineY = Clamp((int)(origin.Y + (radius * Math.Sin(angle))), 0, Game.MapGrid.Height);
        return Game.MapGrid.GetCellAtCoordinate(mineX, mineY);
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

        UpdateCells();
        Debug.Log($"Refreshed cells in {sw.Elapsed}");
    }

    internal void PopulateCell(CellData cell)
    {
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

    private void CreateBuildings(int maxWidth, int maxHeight, int minWidth, int minHeight)
    {
        foreach (var street in Streets)
        {
            for (int cellIndex = 0; cellIndex < street.Count - 1; cellIndex++)
            {
                var cell = street[cellIndex];
                var biggest = new List<CellData>();

                var neighbours = new List<CellData>
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
                            var structure = GetRectangle(neighbour, width, height);
                            var measure = GetWidthAndHeight(structure);

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
                    Buildings.Add(biggest);

                    foreach (var buffer in Grow(biggest, Random.Range(1, 3)))
                    {
                        if (buffer.Structure == null)
                        {
                            buffer.CreateStructure("Reserved");
                        }
                    }
                }
            }
        }

        // cull buildings that are not connected
        foreach (var building in Buildings)
        {
            var v = Random.value;

            if (!Grow(building, 1).Any(c => c.Structure.Name == "Road"))
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
                foreach (var cell in HollowSquare(building))
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

    private void CreateLeyLines()
    {
        var nexusPoints = new List<CellData>();
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
            Game.LeyLineController.MakeLine(Pathfinder.FindPath(cell, target, Mobility.Fly), (ManaColor)v.GetValue(counter));

            counter++;

            if (counter >= v.Length)
            {
                counter = 0;
            }
        }
    }

    private void CreateStreets()
    {
        var mainStreet = GetDiameterLine(Game.MapGrid.Center, Random.Range(30, 90), Random.Range(-10, 10));
        mainStreet.ForEach(c => c.CreateStructure("Road"));
        Streets.Add(mainStreet);

        for (int i = 1; i < mainStreet.Count; i++)
        {
            if (Random.value > 0.8)
            {
                MakeStreet(mainStreet[i], Random.Range(15, 25), true, Random.value * 2, 6);
                i += 5;
            }
        }
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

    private List<CellData> GetCorners(List<CellData> square)
    {
        var minMax = GetMinMax(square);

        return new List<CellData>
            {
                Game.MapGrid.GetCellAtCoordinate(minMax.minx, minMax.miny),
                Game.MapGrid.GetCellAtCoordinate(minMax.minx, minMax.maxy),
                Game.MapGrid.GetCellAtCoordinate(minMax.maxx, minMax.miny),
                Game.MapGrid.GetCellAtCoordinate(minMax.maxx, minMax.maxy)
            };
    }

    private List<CellData> GetNeigbours(CellData cell)
    {
        return Grow(new List<CellData> { cell }, 1);
    }

    private List<CellData> GetPossibleDoors(List<CellData> building)
    {
        var doors = new List<CellData>();
        var corners = GetCorners(building);
        foreach (var cell in GetBorder(building))
        {
            if (corners.Any(c => c.X == cell.X && c.Y == cell.Y))
            {
                continue;
            }

            var neighbours = GetNeigbours(cell).Count(c => c.Structure.Name == "Road");
            if (neighbours > 1)
            {
                doors.Add(cell);
            }
        }

        return doors;
    }

    private List<CellData> Grow(List<CellData> cells, int size)
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

    private void MakeFactionCore(CellData center, Faction faction)
    {
        Game.MapGrid.Center.SetStructure(FactionController.PlayerFaction.Core);
        foreach (var cell in Game.MapGrid.GetCircle(center, 5))
        {
            cell.Binding = faction.Core;
        }
    }

    private void MakeStreet(CellData crossingPoint, int length, bool vertical, double momentum, int color)
    {
        var degrees = vertical ? new[] { 90, 270 } : new[] { 0, 180 };
        var angle = degrees[Random.Range(0, 2)];

        if (Random.value > 0.7)
        {
            angle += Random.Range(-10, 10);
        }

        var street = GetLine(crossingPoint, GetPointAtDistanceOnAngle(crossingPoint, length, angle));

        foreach (var cell in street)
        {
            cell.CreateStructure("Road");
        }

        Streets.Add(street);
        momentum *= Random.value + 1f;
        length = (int)((length * Random.value) + 1f);

        if (momentum > 0.1f)
        {
            for (int i = (int)Math.Ceiling(length / 3f); i < street.Count; i++)
            {
                if (Random.value > 0.9)
                {
                    MakeStreet(street[i], length, !vertical, momentum, color);
                    i += 5;
                }
            }
        }
    }

    private void UpdateCells()
    {
        var cells = Game.MapGrid.Cells;
        foreach (var cell in cells)
        {
            if (cell.Structure != null && cell.Structure.Name == "Reserved")
            {
                Game.StructureController.DestroyStructure(cell.Structure);
            }

            PopulateCell(cell);
        }

        var tiles = cells.Select(c => c.Tile).ToArray();
        var coords = cells.Select(c => c.ToVector3Int()).ToArray();
        Game.MapGrid.Tilemap.SetTiles(coords, tiles);
        Game.StructureController.DrawAllStructures();
    }
}