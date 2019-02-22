using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public enum CellType
{
    Dirt, Forest, Grass, Mountain, Stone, Water
}

public class MapGrid : MonoBehaviour
{
    [Range(4, 300)]
    public int MapSize = 64;

    public SpriteRenderer MapSpriteRenderer;
    internal const float JitterProbability = 0.25f;

    internal List<CellData> Cells { get; set; }

    private static MapGrid _instance;
    private Dictionary<(int x, int y), CellData> _cellLookup;
    private CellPriorityQueue _searchFrontier = new CellPriorityQueue();
    private int _searchFrontierPhase;
    public static MapGrid Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("MapGrid").GetComponent<MapGrid>();
            }

            return _instance;
        }
    }

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

    public void AddCellIfValid(int x, int y, List<CellData> cells)
    {
        if (x >= 0 && x < MapSize && y >= 0 && y < MapSize)
        {
            cells.Add(CellLookup[(x, y)]);
        }
    }

    public void Awake()
    {
        MapSpriteRenderer = GetComponent<SpriteRenderer>();
        CreateMap();
    }
    public CellData CreateCell(int x, int y, CellType type)
    {
        var cell = new CellData
        {
            CellType = type,
            Coordinates = new Coordinates(x, y)
        };

        Cells.Add(cell);

        return cell;
    }

    public void CreateMap()
    {
        Cells = new List<CellData>();

        for (var y = 0; y < MapSize; y++)
        {
            for (var x = 0; x < MapSize; x++)
            {
                CreateCell(x, y, CellType.Water);
            }
        }

        LinkNeighbours();

        // generate bedrock
        for (int i = 0; i < MapSize / 2; i++)
        {
            foreach (var cell in GetRandomChunk(Random.Range(1 + (MapSize / 6), 1 + (MapSize / 3))))
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
        for (int i = 0; i < MapSize; i++)
        {
            foreach (var cell in GetRandomChunk(Random.Range(MapSize, MapSize * 2)))
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

        const int pixelsPerCell = 100;
        var texture = new Texture2D(Instance.MapSize * pixelsPerCell, Instance.MapSize * pixelsPerCell);

        foreach (var cell in Cells)
        {
            var type = cell.CellType;

            var x = cell.Coordinates.X * pixelsPerCell;
            var y = cell.Coordinates.Y * pixelsPerCell;

            var xStart = x;
            var xEnd = x + pixelsPerCell;
            foreach (var pixel in SpriteStore.Instance.GetGroundTextureFor(type.ToString(), pixelsPerCell))
            {
                texture.SetPixel(x, y, pixel);
                x++;

                if (x >= xEnd)
                {
                    x = xStart;
                    y++;
                }
            }

            var value = Random.value;

            switch (cell.CellType)
            {
                case CellType.Grass:
                    cell.TravelCost = 1;
                    if (value > 0.65)
                    {
                        cell.AddContent(StructureController.Instance.GetStructure("Bush").gameObject);
                    }
                    break;
                case CellType.Forest:
                    cell.TravelCost = 1;
                    if (value > 0.95)
                    {
                        cell.AddContent(StructureController.Instance.GetStructure("Tree").gameObject);
                    }
                    else if (value > 0.65)
                    {
                        cell.AddContent(StructureController.Instance.GetStructure("Bush").gameObject);
                    }
                    break;

                case CellType.Stone:
                    cell.TravelCost = 1;
                    for (int i = 0; i < Random.Range(1, 5); i++)
                    {
                        cell.AddContent(ItemController.Instance.GetItem("Rock").gameObject);
                    }
                    break;
            }
        }

        ResetSearchPriorities();

        texture.Apply();
        // System.IO.File.WriteAllBytes("tex.png", texture.EncodeToPNG());

        MapSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, Instance.MapSize * pixelsPerCell, Instance.MapSize * pixelsPerCell), new Vector2(0.5f, 0.5f), pixelsPerCell);

        transform.position = new Vector2(Instance.MapSize / 2, Instance.MapSize / 2);

        MapSpriteRenderer.sprite = MapSprite;

        CreatureController.Instance.SpawnCreatures();


    }

    internal Sprite MapSprite;

    public void Update()
    {
    }

    public CellData GetCellAtCoordinate(Coordinates coordintes)
    {
        return CellLookup[(coordintes.X, coordintes.Y)];
    }

    public CellData GetCellAtPoint(Vector3 position)
    {
        var coordinates = Coordinates.FromPosition(position);
        return CellLookup[(coordinates.X, coordinates.Y)];
    }

    public List<CellData> GetCircle(CellData center, int radius)
    {
        var cells = new List<CellData>();
        var centerX = center.Coordinates.X;
        var centerY = center.Coordinates.Y;

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
        return CellLookup[((int)(Random.value * (MapSize - 1)), (int)(Random.value * (MapSize - 1)))];
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
                    neighbor.SearchHeuristic = Random.value < JitterProbability ? 1 : 0;
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

    public void HighlightCells(List<CellData> cells, Color color)
    {
        foreach (var cell in cells)
        {
            cell.EnableBorder(color);
        }
    }

    internal void ClearCache()
    {
        _cellLookup = null;
    }

    internal void DestroyCell(CellData cell)
    {
        foreach (var item in cell.ContainedItems.ToArray())
        {
            ItemController.Instance.DestroyItem(item);
        }

        if (cell.Structure != null)
        {
            StructureController.Instance.DestroyStructure(cell.Structure);
        }

        if (cell.Stockpile != null)
        {
            StockpileController.Instance.DestroyStockpile(cell.Stockpile);
        }

        //  Destroy(cell.gameObject);
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

    internal Coordinates GetPathableNeighbour(Coordinates coordinates)
    {
        return Instance.GetCellAtCoordinate(coordinates).Neighbors
                                       .First(c => c.TravelCost != 0).Coordinates;
    }

    internal void LinkNeighbours()
    {
        for (var y = 0; y < MapSize; y++)
        {
            for (var x = 0; x < MapSize; x++)
            {
                var cell = CellLookup[(x, y)];

                if (x > 0)
                {
                    cell.SetNeighbor(Direction.W, CellLookup[(x - 1, y)]);

                    if (y > 0)
                    {
                        cell.SetNeighbor(Direction.SW, CellLookup[(x - 1, y - 1)]);

                        if (x < MapSize - 1)
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
        for (var y = 0; y < MapSize; y++)
        {
            for (var x = 0; x < MapSize; x++)
            {
                CellLookup[(x, y)].SearchPhase = 0;
            }
        }
    }
}