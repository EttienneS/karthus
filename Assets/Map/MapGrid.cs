using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public enum CellType
{
    Dirt, Forest, Grass, Mountain, Stone, Water
}

public class MapGrid : MonoBehaviour
{
    public const int PixelsPerCell = 64;
    public Dictionary<string, List<CellData>> CellBinding = new Dictionary<string, List<CellData>>();
    public Text CellLabel;
    [Range(0f, 1f)] public float JitterProbability = 0.8f;
    public float Lancunarity = 2;
    [Range(100, 500)] public int Width = 100;
    [Range(100, 500)] public int Height = 100;
    public int Octaves = 4;
    public Vector2 Offset;
    public Dictionary<string, List<CellData>> PendingBinding = new Dictionary<string, List<CellData>>();

    public Dictionary<string, List<CellData>> PendingUnbinding = new Dictionary<string, List<CellData>>();

    [Range(0f, 1f)] public float Persistance = 0.5f;

    [Range(0.5f, 100f)] public float Scale = 10;

    public int Seed;

    internal SpriteRenderer Background;

    internal Tilemap Tilemap;

    private Dictionary<(int x, int y), CellData> _cellLookup;

    private CellPriorityQueue _searchFrontier = new CellPriorityQueue();

    private int _searchFrontierPhase;

    public Dictionary<(int x, int y), CellData> CellLookup
    {
        get
        {
            if (_cellLookup == null)
            {
                _cellLookup = new Dictionary<(int x, int y), CellData>();

                foreach (var cell in Cells)
                {
                    _cellLookup.Add((cell.X, cell.Y), cell);
                }
            }
            return _cellLookup;
        }
    }

    public CellData Center
    {
        get
        {
            return CellLookup[(Game.MapGrid.Width / 2, Game.MapGrid.Height / 2)];
        }
    }

    internal List<CellData> Cells { get; set; }

    public void AddCellIfValid(int x, int y, List<CellData> cells)
    {
        if (x >= 0 && x < Game.MapGrid.Width && y >= 0 && y < Game.MapGrid.Height)
        {
            cells.Add(CellLookup[(x, y)]);
        }
    }

    public void Awake()
    {
        Tilemap = GetComponentInChildren<Tilemap>();
        Background = GetComponentInChildren<SpriteRenderer>();

        Background.transform.position = new Vector3(Game.MapGrid.Width / 2, Game.MapGrid.Height / 2, 0);
        Background.transform.localScale = new Vector3(Game.MapGrid.Width + 2, Game.MapGrid.Height + 2, 1);
    }

    public void BindCell(CellData cell, IEntity originator)
    {
        if (cell.Bound)
        {
            if (!CellBinding.ContainsKey(originator.Id))
            {
                CellBinding.Add(originator.Id, new List<CellData>());
            }

            if (CellBinding.ContainsKey(cell.Binding.Id))
            {
                CellBinding[cell.Binding.Id].Remove(cell);
            }

            CellBinding[originator.Id].Add(cell);
            cell.Binding = originator;
        }
        else
        {
            if (!PendingBinding.ContainsKey(originator.Id))
            {
                PendingBinding.Add(originator.Id, new List<CellData>());
            }

            PendingBinding[originator.Id].Add(cell);
        }
    }

    public List<CellData> BleedGroup(List<CellData> group, int count, float percentage = 0.7f)
    {
        for (var i = 0; i < count; i++)
        {
            group = BleedGroup(group.ToList(), percentage);
        }

        return group;
    }

    public List<CellData> BleedGroup(List<CellData> group, float percentage = 0.7f)
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

    public CellData GetCellAtCoordinate(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Game.MapGrid.Width || y >= Game.MapGrid.Height)
        {
            return null;
        }

        return CellLookup[(x, y)];
    }

    public CellData GetCellAtPoint(Vector3 position)
    {
        // subtract half a unit to compensate for cell offset
        var cell = CellData.FromPosition(position - new Vector3(0.5f, 0.5f));

        if (cell.X < 0 || cell.Y < 0 ||
            cell.X >= Game.MapGrid.Width || cell.Y >= Game.MapGrid.Height)
        {
            return null;
        }
        return CellLookup[(cell.X, cell.Y)];
    }

    public List<CellData> GetCircle(CellData center, int radius)
    {
        var cells = new List<CellData>();

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

    public CellData GetRandomCell()
    {
        return CellLookup[((int)(Random.value * (Game.MapGrid.Width - 1)), (int)(Random.value * (Game.MapGrid.Height - 1)))];
    }

    public List<CellData> GetRandomChunk(int chunkSize)
    {
        _searchFrontierPhase++;
        var firstCell = GetRandomCell();
        firstCell.SearchPhase = _searchFrontierPhase;
        firstCell.Distance = 0;
        firstCell.SearchHeuristic = 0;
        _searchFrontier.Enqueue(firstCell);

        var center = firstCell;
        int size = 0;

        var chunk = new List<CellData>();
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
                    neighbor.SearchHeuristic = Random.value < Game.MapGrid.JitterProbability ? 1 : 0;
                    _searchFrontier.Enqueue(neighbor);
                }
            }
        }

        _searchFrontier.Clear();

        return chunk;
    }

    public CellData GetRandomPathableCell()
    {
        var cell = GetRandomCell();

        while (cell.TravelCost < 1)
        {
            cell = GetRandomCell();
        }

        return cell;
    }

    public List<CellData> GetRectangle(int x, int y, int width, int height)
    {
        var cells = new List<CellData>();

        for (var i = x; i < x + width; i++)
        {
            for (var k = y; k < y + height; k++)
            {
                AddCellIfValid(i, k, cells);
            }
        }

        return cells;
    }

    public void RefreshCell(CellData cell)
    {
        Tilemap.SetTile(new Vector3Int(cell.X, cell.Y, 0), cell.Tile);
        if (cell.Structure != null)
        {
            Game.StructureController.RefreshStructure(cell.Structure);
        }
    }

    public void Update()
    {
        ProcessBindings(50);
    }

    internal void AddCellLabel(CellData cell)
    {
        //if (Game.MapGrid.WorldCanvas != null)
        //{
        //    var label = Instantiate(CellLabel, WorldCanvas.transform);
        //    label.name = $"CL_{cell.Coordinates}";
        //    label.transform.position = cell.Coordinates.ToTopOfMapVector();
        //    label.text = cell.Coordinates.ToStringOnSeparateLines();
        //}
    }

    internal void ClearCache()
    {
        _cellLookup = null;
    }

    internal void DestroyCell(CellData cell)
    {
        if (cell.Structure != null)
        {
            Game.StructureController.DestroyStructure(cell.Structure);
        }
    }

    internal float GetAngle(CellData c1, CellData c2)
    {
        return Mathf.Atan2(c2.X - c1.X, c2.Y - c1.Y) * 180.0f / Mathf.PI;
    }

    internal Direction GetDirection(CellData fromCell, CellData toCell)
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

    internal CellData GetNearestCellOfType(CellData centerPoint, CellType cellType, int radius)
    {
        return GetCircle(centerPoint, radius)
                    .Where(c => c.Bound && c.CellType == cellType)
                    .OrderBy(c => c.DistanceTo(centerPoint))
                    .First();
    }

    internal CellData GetPathableNeighbour(CellData coordinates)
    {
        return coordinates.Neighbors
                          .Where(c => c.Bound && c.TravelCost > 0)
                          .OrderBy(c => c.TravelCost)
                          .First();
    }

    internal CellData GetRandomRadian(CellData center, int radius)
    {
        var angle = Random.Range(0, 360);
        var mineX = Mathf.Clamp(Mathf.FloorToInt(center.X + (radius * Mathf.Cos(angle))), 0, Game.MapGrid.Width);
        var mineY = Mathf.Clamp(Mathf.FloorToInt(center.Y + (radius * Mathf.Sin(angle))), 0, Game.MapGrid.Height);

        return GetCellAtCoordinate(mineX, mineY);
    }

    internal CellData GetCellAttRadian(CellData center, int radius, int angle)
    {
        var mineX = Mathf.Clamp(Mathf.FloorToInt(center.X + (radius * Mathf.Cos(angle))), 0, Game.MapGrid.Width);
        var mineY = Mathf.Clamp(Mathf.FloorToInt(center.Y + (radius * Mathf.Sin(angle))), 0, Game.MapGrid.Height);

        return GetCellAtCoordinate(mineX, mineY);
    }

    internal void ProcessBindings(int maxDraws)
    {
        var draws = 0;

        var doneBind = new List<string>();
        var doneUnbind = new List<string>();

        foreach (var kvp in PendingBinding)
        {
            List<CellData> done = new List<CellData>();
            foreach (var cell in kvp.Value)
            {
                if (!cell.Bound)
                {
                    cell.Binding = IdService.GetEntityFromId(kvp.Key);

                    if (cell.Binding == null)
                    {
                        doneBind.Add(kvp.Key);
                        kvp.Value.Clear();
                        Debug.LogWarning("Unbindable entity found, clearing.");
                        continue;
                    }

                    if (!CellBinding.ContainsKey(kvp.Key))
                    {
                        CellBinding.Add(kvp.Key, new List<CellData>());
                    }

                    CellBinding[kvp.Key].Add(cell);
                    RefreshCell(cell);
                    if (draws++ > maxDraws)
                    {
                        break;
                    }
                }
                else
                {
                    done.Add(cell);
                }
            }

            done.ForEach(c => kvp.Value.Remove(c));

            if (kvp.Value.Count == 0)
            {
                doneBind.Add(kvp.Key);
            }
        }

        foreach (var kvp in PendingUnbinding)
        {
            var cell = kvp.Value.FirstOrDefault();
            if (cell != null)
            {
                cell.Binding = null;

                if (cell.Structure != null)
                {
                    Game.StructureController.DestroyStructure(cell.Structure);
                }

                if (CellBinding.ContainsKey(kvp.Key))
                {
                    CellBinding[kvp.Key].Remove(cell);
                }

                RefreshCell(cell);

                if (draws++ > maxDraws)
                {
                    break;
                }
            }

            kvp.Value.Remove(cell);

            if (kvp.Value.Count == 0)
            {
                doneUnbind.Add(kvp.Key);
            }
        }

        doneBind.ForEach(r => PendingBinding.Remove(r));
        doneUnbind.ForEach(r => PendingUnbinding.Remove(r));
    }

    internal void Unbind(string id)
    {
        if (CellBinding.ContainsKey(id))
        {
            if (!PendingUnbinding.ContainsKey(id))
            {
                PendingUnbinding.Add(id, new List<CellData>());
            }
            PendingUnbinding[id].AddRange(CellBinding[id]);
        }
    }

    private void OnValidate()
    {
        if (Width < 1)
        {
            Width = 1;
        }

        if (Height < 1)
        {
            Height = 1;
        }

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