using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    public const int PixelsPerCell = 64;
    public Dictionary<(int x, int y), Cell> CellLookup = new Dictionary<(int x, int y), Cell>();
    public List<Cell> Cells = new List<Cell>();
    public ChunkRenderer ChunkPrefab;

    public GameObject WaterPrefab;

    public Light GlobalLight;

    public NoiseSettings LocalNoise;

    public NoiseSettings WorldNoise;

    internal Dictionary<(int x, int y), ChunkRenderer> Chunks;
    private float[,] _localNoiseMap;
    private CellPriorityQueue _searchFrontier = new CellPriorityQueue();
    private int _searchFrontierPhase;
    private int? _seedValue;
    private float[,] _worldNoiseMap;

    public Cell Center
    {
        get
        {
            return CellLookup[((Game.Instance.MapData.Size * Game.Instance.MapData.ChunkSize) / 2, (Game.Instance.MapData.Size * Game.Instance.MapData.ChunkSize) / 2)];
        }
    }

    public float[,] LocalNoiseMap
    {
        get
        {
            if (_localNoiseMap == null)
            {
                _localNoiseMap = Noise.GenerateNoiseMap(SeedValue, Game.Instance.MaxSize, Game.Instance.MaxSize, LocalNoise, Vector2.zero);
            }
            return _localNoiseMap;
        }
    }

    internal int MaxX => Game.Instance.MaxSize;
    internal int MaxZ => Game.Instance.MaxSize;
    internal int MinX => 0;
    internal int MinZ => 0;

    internal int SeedValue
    {
        get
        {
            if (!_seedValue.HasValue)
            {
                var md5Hasher = MD5.Create();
                var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(Game.Instance.MapData.Seed));
                _seedValue = BitConverter.ToInt32(hashed, 0);
            }
            return _seedValue.Value;
        }
    }

    public void AddCellIfValid(int x, int y, List<Cell> cells)
    {
        if (CellLookup.ContainsKey((x, y)))
        {
            cells.Add(CellLookup[(x, y)]);
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

    public Cell GetCellAtCoordinate((float x, float y) coords)
    {
        return GetCellAtCoordinate(coords.x, coords.y);
    }

    public Cell GetCellAtCoordinate(float x, float y)
    {
        var intx = Mathf.RoundToInt(x - 0.001f);
        var inty = Mathf.RoundToInt(y - 0.001f);

        if (intx < MinX || inty < MinZ || intx >= MaxX || inty >= MaxZ)
        {
            return null;
        }

        return CellLookup[(intx, inty)];
    }

    public Cell GetCellAtPoint(Vector3 position)
    {
        // subtract half a unit to compensate for cell offset
        var cell = Cell.FromPosition(position - new Vector3(0.5f, 0.5f));

        if (cell == null ||
            cell.X < MinX || cell.Z < MinZ ||
            cell.X >= MaxX || cell.Z >= MaxZ)
        {
            return null;
        }
        return CellLookup[(cell.X, cell.Z)];
    }

    public float GetNoiseMapPoint(float x, float y)
    {
        return LocalNoiseMap[(int)x, (int)y];
        //return Mathf.PerlinNoise((SeedValue + x) * Scaler, (SeedValue + y) * Scaler);
    }

    public List<Cell> GetCircle(Cell center, int radius)
    {
        var cells = new List<Cell>();

        var centerX = center.X;
        var centerY = center.Z;

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
        var deltaY = point1.Z - point2.Z;

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
        var y = a.Z;
        var x2 = b.X;
        var y2 = b.Z;

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
            line.Add(GetCellAtCoordinate(x, y));
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
            if (cell.Z > maxy)
            {
                maxy = cell.Z;
            }
            if (cell.Z < miny)
            {
                miny = cell.Z;
            }
        }

        return (minx, maxx, miny, maxy);
    }

    public Cell GetPointAtDistanceOnAngle(Cell origin, int distance, float angle)
    {
        var radians = angle * Math.PI / 180.0;

        // cater for right angle scenarios
        var tX = origin.X;
        var tY = origin.Z;

        if (angle != 0 && angle != 180)
        {
            tY = (int)((Math.Sin(-radians) * distance) + origin.Z);
        }

        if (angle != 90 && angle != 270)
        {
            tX = (int)((Math.Cos(radians) * distance) + origin.X);
        }

        // add 1 to offset rounding errors
        return GetCellAtCoordinate(tX, tY);
    }

    public Cell GetRandomCell()
    {
        return CellLookup[((int)(Random.value * (MaxX - 1)), (int)(Random.value * (MaxZ - 1)))];
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
                    neighbor.SearchHeuristic = Random.value < 0.6f ? 1 : 0;
                    _searchFrontier.Enqueue(neighbor);
                }
            }
        }

        _searchFrontier.Clear();

        return chunk;
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

    public List<Cell> GetRectangle(Cell cell1, Cell cell2)
    {
        var x = cell1.X;
        var y = cell1.Z;
        var w = x - cell2.X;
        var h = y - cell2.Z;

        return GetRectangle(x, y, w, h);
    }

    public List<Cell> GetRectangle(int inX, int inY, int width, int height)
    {
        var cells = new List<Cell>();

        var fromX = Math.Min(inX, inX + width);
        var toX = Math.Max(inX, inX + width);
        var fromY = Math.Min(inY, inY + height);
        var toY = Math.Max(inY, inY + height);

        for (var x = fromX; x < toX; x++)
        {
            for (var y = fromY; y < toY; y++)
            {
                AddCellIfValid(x, y, cells);
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
        var src = GetCellAtCoordinate(minMax.minx + 1, minMax.miny + 1);
        return GetRectangle(src.X, src.Z,
                            minMax.maxx - minMax.minx - 1,
                            minMax.maxy - minMax.miny - 1);
    }

    public ChunkRenderer MakeChunk(Chunk data)
    {
        var chunk = Instantiate(ChunkPrefab, transform);
        chunk.transform.position = new Vector2(data.X * Game.Instance.MapData.ChunkSize, data.Z * Game.Instance.MapData.ChunkSize);
        chunk.name = $"Chunk: {data.X}_{data.Z}";
        chunk.Data = data;

        Game.Instance.Map.Chunks.Add((data.X, data.Z), chunk);

        return chunk;
    }

    internal void DestroyCell(Cell cell)
    {
        if (cell.Structure != null)
        {
            Game.Instance.StructureController.DestroyStructure(cell.Structure);
        }
    }

    internal float GetAngle(Cell c1, Cell c2)
    {
        return Mathf.Atan2(c2.X - c1.X, c2.Z - c1.Z) * 180.0f / Mathf.PI;
    }

    internal Cell GetCellAtCoordinate(Vector3 pos)
    {
        return GetCellAtCoordinate(pos.x, pos.y);
    }

    internal Cell GetCellAtCoordinate(Vector2 pos)
    {
        return GetCellAtCoordinate(pos.x, pos.y);
    }

    internal Cell GetCellAttRadian(Cell center, int radius, int angle)
    {
        var mineX = Mathf.Clamp(Mathf.FloorToInt(center.X + (radius * Mathf.Cos(angle))), 0, MaxX);
        var mineY = Mathf.Clamp(Mathf.FloorToInt(center.Z + (radius * Mathf.Sin(angle))), 0, MaxZ);

        return GetCellAtCoordinate(mineX, mineY);
    }

    internal Direction GetDirection(Cell fromCell, Cell toCell)
    {
        var direction = Direction.S;

        if (fromCell != null && toCell != null)
        {
            var x = fromCell.X - toCell.X;
            var y = fromCell.Z - toCell.Z;

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

    internal List<Cell> GetEdge(List<Cell> cells)
    {
        return cells.Where(c => c.Neighbors.Any(n => n != null && !cells.Contains(n))).ToList();
    }

    internal Cell GetNearestEmptyCell(Cell cell)
    {
        var circle = GetCircle(cell, 1);

        for (int i = 2; i < 15; i++)
        {
            var newCircle = GetCircle(cell, i);
            newCircle.RemoveAll(c => circle.Contains(c));
            circle = newCircle;

            var empty = circle.Where(c => c.Structure == null);

            if (empty.Count() > 0)
            {
                return empty.GetRandomItem();
            }
        }

        return null;
    }

    internal Cell GetNearestPathableCell(Cell centerPoint, Mobility mobility, int radius)
    {
        var circle = GetCircle(centerPoint, radius);

        return circle.Where(c => c != centerPoint && c.Pathable(mobility))
                           .OrderBy(c => c.DistanceTo(centerPoint))
                           .First();
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

    internal Cell GetRandomRadian(Cell center, int radius)
    {
        var angle = Random.Range(0, 360);
        var mineX = Mathf.Clamp(Mathf.FloorToInt(center.X + (radius * Mathf.Cos(angle))), 0, MaxX);
        var mineY = Mathf.Clamp(Mathf.FloorToInt(center.Z + (radius * Mathf.Sin(angle))), 0, MaxZ);

        return GetCellAtCoordinate(mineX, mineY);
    }

    

    internal Cell TryGetPathableNeighbour(Cell coordinates)
    {
        var pathables = coordinates.NonNullNeighbors
                     .Where(c => c.TravelCost > 0)
                     .ToList();

        if (pathables.Count > 0)
        {
            return pathables.GetRandomItem();
        }
        return null;
    }
}