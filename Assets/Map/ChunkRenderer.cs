using Assets.Helpers;
using Assets.Sprites;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    public List<Cell> Cells;
    public Chunk Data;
    public MeshRenderer MeshRenderer;
    private Mesh mesh;
    private MeshCollider meshCollider;

    private int triangleIndex;
    private int[] triangles;
    private Vector2[] uvs;
    private Vector3[] vertices;
    public int ChunkSize => Game.Instance.Map.ChunkSize;

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;

        triangleIndex += 3;
    }

    public Cell CreateCell(int x, int y)
    {
        var cell = new Cell
        {
            X = x,
            Y = y,
            SearchPhase = 0,
            Chunk = Data
        };

        Game.Instance.Map.CellLookup.Add((x, y), cell);

        return cell;
    }

    public Vector3Int GetChunkCoordinate(Cell cell)
    {
        return new Vector3Int(cell.X % Game.Instance.Map.ChunkSize, cell.Y % Game.Instance.Map.ChunkSize, 0);
    }

    public Color GetColor(Cell cell)
    {
        switch (cell.BiomeRegion.SpriteName)
        {
            case "Dirt":
                return ColorConstants.YellowBase;

            case "Forest":
                return ColorConstants.GreenBase;

            case "Grass":
                return ColorConstants.GreenAccent;

            case "PatchyGrass":
                return ColorConstants.GreenBase;

            case "Sand":
                return ColorConstants.YellowAccent;

            case "Stone":
                return ColorConstants.GreyBase;

            case "Water":
                return ColorConstants.BlueBase;

            default:
                throw new KeyNotFoundException(cell.BiomeRegion.SpriteName + " not found");
        }
    }

    public void LinkToChunk(ChunkRenderer chunk)
    {
        // link edges to the given chunk edges
        var size = Game.Instance.Map.ChunkSize;

        var firstX = (Data.X * size);
        var firstY = (Data.Y * size);

        if (chunk.Data.X < Data.X)
        {
            // link to chunk on the left (west)
            foreach (var cell in Cells.Where(c => c.X == firstX))
            {
                cell.SetNeighbor(Direction.W, Game.Instance.Map.CellLookup[(cell.X - 1, cell.Y)]);
            }
        }
        else if (chunk.Data.X > Data.X)
        {
            // link to chunk on the right (east)
            foreach (var cell in Cells.Where(c => c.X == firstX + size))
            {
                cell.SetNeighbor(Direction.E, Game.Instance.Map.CellLookup[(cell.X + 1, cell.Y)]);
            }
        }
        else if (chunk.Data.Y < Data.Y)
        {
            // link to chunk below (south)
            foreach (var cell in Cells.Where(c => c.Y == firstY))
            {
                cell.SetNeighbor(Direction.S, Game.Instance.Map.CellLookup[(cell.X, cell.Y - 1)]);
            }
        }
        else if (chunk.Data.Y > Data.Y)
        {
            // link to chunk above (north)
            foreach (var cell in Cells.Where(c => c.Y == firstY))
            {
                cell.SetNeighbor(Direction.N, Game.Instance.Map.CellLookup[(cell.X, cell.Y + 1)]);
            }
        }
    }

    public void Start()
    {
        using (Instrumenter.Start())
        {
            transform.position = new Vector3(Data.X * Game.Instance.Map.ChunkSize, Data.Y * Game.Instance.Map.ChunkSize);
            Populate();
            UpdateInterlocked();
        }

        Triangulate();
        UpdateTexture();
    }

    public void Triangulate()
    {
        using (Instrumenter.Start())
        {
            mesh.Clear();

            uvs = new Vector2[ChunkSize * ChunkSize];
            vertices = new Vector3[ChunkSize * ChunkSize];
            triangles = new int[(ChunkSize - 1) * (ChunkSize - 1) * 6];

            var vertIndex = 0;
            for (int y = 0; y < ChunkSize; y++)
            {
                for (int x = 0; x < ChunkSize; x++)
                {
                    var cell = Game.Instance.Map.GetCellAtCoordinate(x + (Data.X * ChunkSize), y + (Data.Y * ChunkSize));

                    vertices[vertIndex] = new Vector3(x, y, cell.RenderHeight);
                    uvs[vertIndex] = new Vector2(x / (float)ChunkSize, y / (float)ChunkSize);
                    if (x < ChunkSize - 1 && y < ChunkSize - 1)
                    {
                        AddTriangle(vertIndex + ChunkSize, vertIndex + ChunkSize + 1, vertIndex);
                        AddTriangle(vertIndex + 1, vertIndex, vertIndex + ChunkSize + 1);
                    }
                    vertIndex++;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            meshCollider.sharedMesh = mesh;
        }
    }
    public void UpdateTexture()
    {
        var colors = new Color[ChunkSize, ChunkSize];
        for (int y = 0; y < ChunkSize; y++)
        {
            for (int x = 0; x < ChunkSize; x++)
            {
                var cell = Game.Instance.Map.GetCellAtCoordinate(x + (Data.X * ChunkSize), y + (Data.Y * ChunkSize));
                colors[x, y] = GetColor(cell);
            }
        }

        var mats = MeshRenderer.materials;
        mats[0].mainTexture = TextureCreator.CreateTextureFromColorMap(ChunkSize, ChunkSize, colors);
        MeshRenderer.materials = mats;
    }


    internal void MakeCells()
    {
        var size = Game.Instance.Map.ChunkSize;

        var firstX = (Data.X * size);
        var firstY = (Data.Y * size);

        Cells = new List<Cell>();

        for (var y = firstY; y < firstY + size; y++)
        {
            for (var x = firstX; x < firstX + size; x++)
            {
                Cells.Add(CreateCell(x, y));
            }
        }

        // link cell to the others in the chunk
        for (var y = firstY; y < firstY + size; y++)
        {
            for (var x = firstX; x < firstX + size; x++)
            {
                var cell = Game.Instance.Map.CellLookup[(x, y)];
                if (x > firstX)
                {
                    cell.SetNeighbor(Direction.W, Game.Instance.Map.CellLookup[(x - 1, y)]);

                    if (y > firstY)
                    {
                        cell.SetNeighbor(Direction.SW, Game.Instance.Map.CellLookup[(x - 1, y - 1)]);

                        if (x < firstX - 1)
                        {
                            cell.SetNeighbor(Direction.SE, Game.Instance.Map.CellLookup[(x + 1, y - 1)]);
                        }
                    }
                }

                if (y > firstY)
                {
                    cell.SetNeighbor(Direction.S, Game.Instance.Map.CellLookup[(x, y - 1)]);
                }
            }
        }
    }

    internal void Populate()
    {
        Cells.ForEach(c => c.Populate());
        Data.Populated = true;
    }

    internal void SetTile(int x, int y, Tile tile)
    {
        var pos = new Vector3Int(x % Game.Instance.Map.ChunkSize, y % Game.Instance.Map.ChunkSize, 0);
    }

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = $"Mesh {name}";
        meshCollider = gameObject.AddComponent<MeshCollider>();
    }

    private void UpdateInterlocked()
    {
        foreach (var wall in Cells.Where(c => c.Structure?.IsInterlocking() == true).Select(c => c.Structure))
        {
            wall.UpdateInterlocking();
        }
    }
}