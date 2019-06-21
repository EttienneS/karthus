using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum CellType
{
    Abyss, Dirt, Forest, Grass, Mountain, Stone, Water
}

public class MapGrid : MonoBehaviour
{
    public Dictionary<string, List<CellData>> CellBinding = new Dictionary<string, List<CellData>>();
    public Dictionary<string, List<CellData>> PendingBinding = new Dictionary<string, List<CellData>>();
    public Dictionary<string, List<CellData>> PendingUnbinding = new Dictionary<string, List<CellData>>();

    internal Tilemap Tilemap;
    internal Canvas WorldCanvas;

    public Text CellLabel;

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
                    _cellLookup.Add((cell.Coordinates.X, cell.Coordinates.Y), cell);
                }
            }
            return _cellLookup;
        }
    }

    internal List<CellData> Cells { get; set; }

    public void AddCellIfValid(int x, int y, List<CellData> cells)
    {
        if (x >= 0 && x < MapConstants.MapSize && y >= 0 && y < MapConstants.MapSize)
        {
            cells.Add(CellLookup[(x, y)]);
        }
    }

    public void Awake()
    {
        Tilemap = GetComponentInChildren<Tilemap>();
        WorldCanvas = GetComponentInChildren<Canvas>();
    }

    public void BindCell(CellData cell, string binderId)
    {
        if (cell.Bound)
        {
            if (!CellBinding.ContainsKey(binderId))
            {
                CellBinding.Add(binderId, new List<CellData>());
            }

            if (CellBinding.ContainsKey(cell.Binding))
            {
                CellBinding[cell.Binding].Remove(cell);
            }

            CellBinding[binderId].Add(cell);
            cell.Binding = binderId;
        }
        else
        {
            if (!PendingBinding.ContainsKey(binderId))
            {
                PendingBinding.Add(binderId, new List<CellData>());
            }

            PendingBinding[binderId].Add(cell);
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

    public CellData CreateCell(int x, int y, CellType type)
    {
        var cell = new CellData
        {
            CellType = type,
            Coordinates = new Coordinates(x, y)
        };

        Cells.Add(cell);
        RefreshCell(cell);

        if (WorldCanvas != null)
        {
            var label = Instantiate(CellLabel, WorldCanvas.transform);
            label.name = $"CL_{cell.Coordinates}";
            label.transform.position = cell.Coordinates.ToTopOfMapVector();
            label.text = cell.Coordinates.ToStringOnSeparateLines();
        }

        return cell;
    }

    public void CreateMap()
    {
        Cells = new List<CellData>();

        for (var y = 0; y < MapConstants.MapSize; y++)
        {
            for (var x = 0; x < MapConstants.MapSize; x++)
            {
                CreateCell(x, y, CellType.Water);
            }
        }

        LinkNeighbours();
        GenerateMapCells();
        Populatecells();

        ResetSearchPriorities();
    }

    public CellData GetCellAtCoordinate(Coordinates coordintes)
    {
        return CellLookup[(coordintes.X, coordintes.Y)];
    }

    public CellData GetCellAtPoint(Vector3 position)
    {
        // subtract half a unit to compensate for cell offset
        var coordinates = Coordinates.FromPosition(position - new Vector3(0.5f, 0.5f));

        if (coordinates.X < 0 || coordinates.Y < 0 || coordinates.X >= MapConstants.MapSize || coordinates.Y >= MapConstants.MapSize)
        {
            return null;
        }
        return CellLookup[(coordinates.X, coordinates.Y)];
    }

    public List<CellData> GetCircle(Coordinates center, int radius)
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
        return CellLookup[((int)(Random.value * (MapConstants.MapSize - 1)), (int)(Random.value * (MapConstants.MapSize - 1)))];
    }

    public List<CellData> GetRandomChunk(int chunkSize)
    {
        _searchFrontierPhase++;
        var firstCell = GetRandomCell();
        firstCell.SearchPhase = _searchFrontierPhase;
        firstCell.Distance = 0;
        firstCell.SearchHeuristic = 0;
        _searchFrontier.Enqueue(firstCell);

        var center = firstCell.Coordinates;
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
                    neighbor.Distance = neighbor.Coordinates.DistanceTo(center);
                    neighbor.SearchHeuristic = Random.value < MapConstants.JitterProbability ? 1 : 0;
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

    public void Start()
    {
        StartCoroutine(UpdateCells());
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

    internal Direction GetDirection(CellData fromCell, CellData toCell)
    {
        var direction = Direction.S;

        if (fromCell != null && toCell != null)
        {
            var x = fromCell.Coordinates.X - toCell.Coordinates.X;
            var y = fromCell.Coordinates.Y - toCell.Coordinates.Y;

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

    internal CellData GetNearestCellOfType(Coordinates centerPoint, CellType cellType, int radius)
    {
        return GetCircle(centerPoint, radius)
                    .Where(c => c.Bound && c.CellType == cellType)
                    .OrderBy(c => c.Coordinates.DistanceTo(centerPoint))
                    .First();
    }

    internal Coordinates GetPathableNeighbour(Coordinates coordinates)
    {
        return GetCellAtCoordinate(coordinates).Neighbors
                                       .Where(c => c.Bound && c.TravelCost > 0)
                                       .OrderBy(c => c.TravelCost)
                                       .First().Coordinates;
    }

    internal void LinkNeighbours()
    {
        for (var y = 0; y < MapConstants.MapSize; y++)
        {
            for (var x = 0; x < MapConstants.MapSize; x++)
            {
                var cell = CellLookup[(x, y)];

                if (x > 0)
                {
                    cell.SetNeighbor(Direction.W, CellLookup[(x - 1, y)]);

                    if (y > 0)
                    {
                        cell.SetNeighbor(Direction.SW, CellLookup[(x - 1, y - 1)]);

                        if (x < MapConstants.MapSize - 1)
                        {
                            cell.SetNeighbor(Direction.SE, CellLookup[(x + 1, y - 1)]);
                        }
                    }
                }

                if (y > 0)
                {
                    cell.SetNeighbor(Direction.S, CellLookup[(x, y - 1)]);
                }
            }
        }
    }

    internal void ResetSearchPriorities()
    {
        // ensure that all cells have their phases reset
        for (var y = 0; y < MapConstants.MapSize; y++)
        {
            for (var x = 0; x < MapConstants.MapSize; x++)
            {
                CellLookup[(x, y)].SearchPhase = 0;
            }
        }
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

    private static void PopulateCell(CellData cell)
    {
        var value = Random.value;
        var world = FactionController.Factions[FactionConstants.World];
        switch (cell.CellType)
        {
            case CellType.Grass:
                if (value > 0.65)
                {
                    cell.AddContent(Game.StructureController.GetStructure("Bush", world).gameObject);
                }
                break;

            case CellType.Forest:
                if (value > 0.95)
                {
                    cell.AddContent(Game.StructureController.GetStructure("Tree", world).gameObject);
                }
                else if (value > 0.65)
                {
                    cell.AddContent(Game.StructureController.GetStructure("Bush", world).gameObject);
                }
                break;
        }
    }

    private void GenerateMapCells()
    {
        // generate bedrock
        for (int i = 0; i < MapConstants.MapSize / 2; i++)
        {
            foreach (var cell in GetRandomChunk(Random.Range(1 + (MapConstants.MapSize / 6), 1 + (MapConstants.MapSize / 3))))
            {
                cell.CellType = CellType.Stone;
            }
        }

        // grow mountains
        foreach (var cell in Cells)
        {
            if (cell.CellType != CellType.Stone)
            {
                continue;
            }

            if (cell.CountNeighborsOfType(null) +
                cell.CountNeighborsOfType(CellType.Mountain) +
                cell.CountNeighborsOfType(CellType.Stone) > 6)
            {
                cell.CellType = CellType.Mountain;
            }
        }

        // generate landmasses
        for (int i = 0; i < MapConstants.MapSize; i++)
        {
            foreach (var cell in GetRandomChunk(Random.Range(MapConstants.MapSize, MapConstants.MapSize * 2)))
            {
                if (cell.CellType == CellType.Water)
                {
                    cell.CellType = CellType.Grass;
                }
            }
        }

        // bleed water, this enlarges bodies of water
        // creates more natural looking coastlines/rivers
        foreach (var cell in Cells)
        {
            if (cell.CellType == CellType.Water)
            {
                continue;
            }

            var waterN = cell.CountNeighborsOfType(CellType.Water);
            if (waterN > 2 && Random.value > 1.0f - (waterN / 10f))
            {
                cell.CellType = CellType.Water;
            }
        }

        // create coast
        foreach (var cell in Cells)
        {
            if (cell.CellType != CellType.Grass)
            {
                // already water skip
                continue;
            }

            if (cell.CountNeighborsOfType(CellType.Water) > 0)
            {
                cell.CellType = CellType.Dirt;
            }
        }

        // bleed desert
        foreach (var cell in Cells)
        {
            if (cell.CellType == CellType.Water)
            {
                // already water skip
                continue;
            }

            if (cell.CountNeighborsOfType(CellType.Dirt) > 2 && Random.value > 0.5)
            {
                cell.CellType = CellType.Dirt;
            }
        }

        // create forest
        foreach (var cell in Cells)
        {
            if (cell.CellType == CellType.Water)
            {
                // water skip
                continue;
            }

            if (cell.CountNeighborsOfType(null) +
                cell.CountNeighborsOfType(CellType.Grass) +
                cell.CountNeighborsOfType(CellType.Forest) > 6
                && Random.value > 0.2)
            {
                cell.CellType = CellType.Forest;
            }
        }
    }

    private void Populatecells()
    {
        foreach (var cell in Cells)
        {
            PopulateCell(cell);
        }
    }

    private IEnumerator UpdateCells()
    {
        const int maxDraws = 25;
        while (true)
        {
            var draws = 0;
            foreach (var kvp in PendingBinding)
            {
                List<CellData> done = new List<CellData>();
                foreach (var cell in kvp.Value)
                {
                    if (!cell.Bound)
                    {
                        cell.Binding = kvp.Key;

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
            }

            if (draws < maxDraws && Random.value > 0.9f)
            {
                var breaker = 0;
                for (int i = 0; i < maxDraws - draws; i++)
                {
                    var cell = GetRandomCell();

                    if (!cell.Bound || cell.CellType == CellType.Water)
                    {
                        RefreshCell(cell);
                    }
                    else
                    {
                        i--;
                        breaker++;
                    }

                    if (breaker > 100)
                    {
                        // after a 100 misses stop trying
                        break;
                    }
                }
            }

            yield return null;
        }
    }

    private void RefreshCell(CellData cell)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = Game.SpriteStore.GetSpriteForTerrainType(cell.Bound ? cell.CellType : CellType.Abyss);

        Tilemap.SetTile(new Vector3Int(cell.Coordinates.X, cell.Coordinates.Y, 0), tile);

        cell.ColorStructure();
    }
}