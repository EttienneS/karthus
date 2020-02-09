using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class TownGenerator
{
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