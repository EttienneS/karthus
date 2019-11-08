using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public enum CellType
{
    Dirt, Forest, Grass, Mountain, Stone, Water,
    Void
}

public class Map : MonoBehaviour
{
    public const int PixelsPerCell = 64;
    [Range(5, 2000)] public int Height = 100;
    [Range(0f, 1f)] public float JitterProbability = 0.8f;
    public float Lancunarity = 2;
    public int Octaves = 4;
    public Vector2 Offset;
    [Range(0f, 1f)] public float Persistance = 0.5f;
    [Range(0.5f, 100f)] public float Scale = 10;
    public int Seed;
    [Range(5, 2000)] public int Width = 100;
    internal SpriteRenderer Background;

    internal Tilemap Tilemap;
    internal Tilemap LiquidMap;

    private Dictionary<(int x, int y), Cell> _cellLookup;

    private CellPriorityQueue _searchFrontier = new CellPriorityQueue();

    private int _searchFrontierPhase;

    public Dictionary<(int x, int y), Cell> CellLookup
    {
        get
        {
            if (_cellLookup == null)
            {
                _cellLookup = new Dictionary<(int x, int y), Cell>();

                foreach (var cell in Cells)
                {
                    _cellLookup.Add((cell.X, cell.Y), cell);
                }
            }
            return _cellLookup;
        }
    }

    public Cell Center
    {
        get
        {
            return CellLookup[(Game.Map.Width / 2, Game.Map.Height / 2)];
        }
    }

    internal List<Cell> Cells { get; set; }

    public void AddCellIfValid(int x, int y, List<Cell> cells)
    {
        if (x >= 0 && x < Game.Map.Width && y >= 0 && y < Game.Map.Height)
        {
            cells.Add(CellLookup[(x, y)]);
        }
    }

    public void Awake()
    {
        Tilemap = transform.Find("Tilemap").gameObject.GetComponent<Tilemap>();
        LiquidMap = transform.Find("Liquid Map").gameObject.GetComponent<Tilemap>();
        Background = GetComponentInChildren<SpriteRenderer>();

        Background.transform.position = new Vector3(Game.Map.Width / 2, Game.Map.Height / 2, 0);
        Background.transform.localScale = new Vector3(Game.Map.Width + 2, Game.Map.Height + 2, 1);
    }

    internal void Unbind(Structure structure)
    {
        foreach (var cell in Cells.Where(c => c.Binding == structure))
        {
            cell.Unbind();
        }
    }

    public List<Cell> BleedGroup(List<Cell> group, int count, float percentage = 0.7f)
    {
        for (var i = 0; i < count; i++)
        {
            group = BleedGroup(group.ToList(), percentage);
        }

        return group;
    }

    public List<Cell> BleedGroup(List<Cell> group, float percentage = 0.7f)
    {
        var newGroup = group.ToList();

        foreach (var cell in group)
        {
            foreach (var neighbour in cell.Neighbors.Where(n => n != null && !group.Contains(n)))
            {
                if (Random.value > percentage)
                {
                    newGroup.Add(neighbour);
                }
            }
        }

        return newGroup.Distinct().ToList();
    }

    public List<Cell> GetBorder(List<Cell> square)
    {
        var frame = square.ToList();
        var hollow = HollowSquare(square);
        frame.RemoveAll(c => hollow.Contains(c));

        return frame;
    }

    public Cell GetCellAtCoordinate(float x, float y)
    {
        var intx = Mathf.RoundToInt(x - 0.001f);
        var inty = Mathf.RoundToInt(y - 0.001f);

        if (intx < 0 || inty < 0 || intx >= Game.Map.Width || inty >= Game.Map.Height)
        {
            return null;
        }

        return CellLookup[(intx, inty)];
    }

    public Cell GetCellAtPoint(Vector3 position)
    {
        // subtract half a unit to compensate for cell offset
        var cell = Cell.FromPosition(position - new Vector3(0.5f, 0.5f));

        if (cell.X < 0 || cell.Y < 0 ||
            cell.X >= Game.Map.Width || cell.Y >= Game.Map.Height)
        {
            return null;
        }
        return CellLookup[(cell.X, cell.Y)];
    }

    public List<Cell> GetCircle(Cell center, int radius)
    {
        var cells = new List<Cell>();

        var centerX = center.X;
        var centerY = center.Y;

        for (var x = centerX - radius; x <= centerX; x++)
        {
            for (var y = centerY - radius; y <= centerY; y++)
            {
                if ((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY) <= radius * radius)
                {
                    // calculate and add cell for each of the four points
                    AddCellIfValid(centerX - (x - centerX), centerY - (y - centerY), cells);
                    AddCellIfValid(centerX + (x - centerX), centerY + (y - centerY), cells);
                    AddCellIfValid(centerX - (x - centerX), centerY + (y - centerY), cells);
                    AddCellIfValid(centerX + (x - centerX), centerY - (y - centerY), cells);
                }
            }
        }

        return cells;
    }

    public float GetDegreesBetweenPoints(Cell point1, Cell point2)
    {
        var deltaX = point1.X - point2.X;
        var deltaY = point1.Y - point2.Y;

        var radAngle = Math.Atan2(deltaY, deltaX);
        var degreeAngle = radAngle * 180.0 / Math.PI;

        return (float)(180.0 - degreeAngle);
    }

    public List<Cell> GetDiameterLine(Cell center, int lenght, int angle = 0)
    {
        return GetLine(GetPointAtDistanceOnAngle(center, lenght / 2, angle),
                       GetPointAtDistanceOnAngle(center, lenght / 2, angle + 180));
    }

    public List<Cell> GetLine(Cell a, Cell b)
    {
        // Bresenham line algorithm [https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm]
        // https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm

        var line = new List<Cell>();

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
            line.Add(Game.Map.GetCellAtCoordinate(x, y));
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

    public (int minx, int maxx, int miny, int maxy) GetMinMax(List<Cell> cells)
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

    public Cell GetPointAtDistanceOnAngle(Cell origin, int distance, float angle)
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
        return Game.Map.GetCellAtCoordinate(tX, tY);
    }

    public Cell GetRandomCell()
    {
        return CellLookup[((int)(Random.value * (Game.Map.Width - 1)), (int)(Random.value * (Game.Map.Height - 1)))];
    }

    public List<Cell> GetRandomChunk(int chunkSize, Cell origin)
    {
        _searchFrontierPhase++;
        var firstCell = origin;
        firstCell.SearchPhase = _searchFrontierPhase;
        firstCell.Distance = 0;
        firstCell.SearchHeuristic = 0;
        _searchFrontier.Enqueue(firstCell);

        var center = firstCell;
        int size = 0;

        var chunk = new List<Cell>();
        while (size < chunkSize && _searchFrontier.Count > 0)
        {
            var current = _searchFrontier.Dequeue();
            chunk.Add(current);
            size++;

            for (var d = Direction.N; d <= Direction.NW; d++)
            {
                var neighbor = current.GetNeighbor(d);
                if (neighbor != null && neighbor.SearchPhase < _searchFrontierPhase)
                {
                    neighbor.SearchPhase = _searchFrontierPhase;
                    neighbor.Distance = neighbor.DistanceTo(center);
                    neighbor.SearchHeuristic = Random.value < Game.Map.JitterProbability ? 1 : 0;
                    _searchFrontier.Enqueue(neighbor);
                }
            }
        }

        _searchFrontier.Clear();

        return chunk;
    }

    internal List<Cell> GetEdge(List<Cell> cells)
    {
        return cells.Where(c => c.Neighbors.Any(n => n != null && !cells.Contains(n))).ToList();
    }

    public Cell GetRandomPathableCell()
    {
        var cell = GetRandomCell();

        while (cell.TravelCost < 1)
        {
            cell = GetRandomCell();
        }

        return cell;
    }

    public List<Cell> GetRectangle(int x, int y, int width, int height)
    {
        var cells = new List<Cell>();

        for (var i = x; i < x + width; i++)
        {
            for (var k = y; k < y + height; k++)
            {
                AddCellIfValid(i, k, cells);
            }
        }

        return cells;
    }

    public List<Cell> GetRectangle(Cell cell, int width, int height)
    {
        var cells = new List<Cell>();

        var fromX = Math.Min(cell.X, cell.X + width);
        var toX = Math.Max(cell.X, cell.X + width);

        var fromY = Math.Min(cell.Y, cell.Y + height);
        var toY = Math.Max(cell.Y, cell.Y + height);

        for (var x = fromX; x < toX; x++)
        {
            for (var y = fromY; y < toY; y++)
            {
                Game.Map.AddCellIfValid(x, y, cells);
            }
        }

        return cells;
    }

    public (int, int) GetWidthAndHeight(List<Cell> cells)
    {
        var minMax = GetMinMax(cells);
        return (minMax.maxx - minMax.minx, minMax.maxy - minMax.miny);
    }

    public List<Cell> HollowSquare(List<Cell> square)
    {
        var minMax = GetMinMax(square);
        var src = Game.Map.GetCellAtCoordinate(minMax.minx + 1, minMax.miny + 1);
        return GetRectangle(src,
                            minMax.maxx - minMax.minx - 1,
                            minMax.maxy - minMax.miny - 1);
    }

    internal void ClearCache()
    {
        _cellLookup = null;
    }

    internal void DestroyCell(Cell cell)
    {
        if (cell.Structure != null)
        {
            Game.StructureController.DestroyStructure(cell.Structure);
        }
    }

    internal float GetAngle(Cell c1, Cell c2)
    {
        return Mathf.Atan2(c2.X - c1.X, c2.Y - c1.Y) * 180.0f / Mathf.PI;
    }

    internal Cell GetCellAttRadian(Cell center, int radius, int angle)
    {
        var mineX = Mathf.Clamp(Mathf.FloorToInt(center.X + (radius * Mathf.Cos(angle))), 0, Game.Map.Width);
        var mineY = Mathf.Clamp(Mathf.FloorToInt(center.Y + (radius * Mathf.Sin(angle))), 0, Game.Map.Height);

        return GetCellAtCoordinate(mineX, mineY);
    }

    internal Direction GetDirection(Cell fromCell, Cell toCell)
    {
        var direction = Direction.S;

        if (fromCell != null && toCell != null)
        {
            var x = fromCell.X - toCell.X;
            var y = fromCell.Y - toCell.Y;

            if (x < 0 && y == 0)
            {
                direction = Direction.E;
            }
            else if (x < 0 && y > 0)
            {
                direction = Direction.NE;
            }
            else if (x < 0 && y < 0)
            {
                direction = Direction.SE;
            }
            else if (x > 0 && y == 0)
            {
                direction = Direction.W;
            }
            else if (x > 0 && y > 0)
            {
                direction = Direction.NW;
            }
            else if (x > 0 && y < 0)
            {
                direction = Direction.SW;
            }
            else if (y < 0)
            {
                direction = Direction.N;
            }
        }

        return direction;
    }

    internal Cell GetRandomEmptyCell()
    {
        Cell cell = null;

        while (cell == null)
        {
            cell = GetRandomCell();

            if (cell.Floor != null || cell.Structure != null)
            {
                cell = null;
            }
        }

        return cell;
    }

    internal Cell GetNearestCellOfType(Cell centerPoint, CellType cellType, int radius)
    {
        return GetCircle(centerPoint, radius)
                    .Where(c => c.Bound && c.CellType == cellType)
                    .OrderBy(c => c.DistanceTo(centerPoint))
                    .First();
    }

    internal Cell GetPathableNeighbour(Cell coordinates)
    {
        return coordinates.Neighbors
                          .Where(c => c.Bound && c.TravelCost > 0)
                          .ToList()
                          .GetRandomItem();
    }

    internal Cell GetRandomRadian(Cell center, int radius)
    {
        var angle = Random.Range(0, 360);
        var mineX = Mathf.Clamp(Mathf.FloorToInt(center.X + (radius * Mathf.Cos(angle))), 0, Game.Map.Width);
        var mineY = Mathf.Clamp(Mathf.FloorToInt(center.Y + (radius * Mathf.Sin(angle))), 0, Game.Map.Height);

        return GetCellAtCoordinate(mineX, mineY);
    }

    internal void Refresh(RectInt rect)
    {
        var cells = Game.Map.GetRectangle(rect.x,
                                              rect.y,
                                              rect.width,
                                              rect.height).Where(c => !c.DrawnOnce).ToList();

        if (cells.Count > 0)
        {
            cells.ForEach(c => Game.MapGenerator.PopulateCell(c));
            var tiles = cells.Select(c => c.Tile).ToArray();
            var coords = cells.Select(c => c.ToVector3Int()).ToArray();
            Game.Map.Tilemap.SetTiles(coords, tiles);
            Game.StructureController.DrawAllStructures(cells);
        }
    }

    private void OnValidate()
    {
        if (Lancunarity < 1)
        {
            Lancunarity = 1;
        }

        if (Octaves < 1)
        {
            Octaves = 1;
        }
    }
}