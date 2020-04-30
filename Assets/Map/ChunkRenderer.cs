using Assets.Sprites;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    #region Mesh

    private Mesh mesh;
    private MeshCollider meshCollider;

    private int[] triangles;
    private Vector2[] uvs;
    private Vector3[] vertices;
    private Color[] colors;

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

    private int triangleIndex;

    public void AddTriangle(int a, int b, int c, Color color)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;

        triangleIndex += 3;
    }

    public void Triangulate()
    {
        using (Instrumenter.Init())
        {
            mesh.Clear();

            var width = Game.Instance.Map.ChunkSize;
            var height = Game.Instance.Map.ChunkSize;

            uvs = new Vector2[width * height];
            vertices = new Vector3[width * height];
            triangles = new int[(width - 1) * (height - 1) * 6];

            var colors = new Color[width, height];
            var vertIndex = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = Game.Instance.Map.GetCellAtCoordinate(x + (Data.X * width), y + (Data.Y * height));
                    colors[x, y] = GetColor(cell);

                    vertices[vertIndex] = new Vector3(x, y, cell.RenderHeight);
                    uvs[vertIndex] = new Vector2(x / (float)width, y / (float)height);
                    if (x < width - 1 && y < height - 1)
                    {
                        AddTriangle(vertIndex + width, vertIndex + width + 1, vertIndex, cell.Color);
                        AddTriangle(vertIndex + 1, vertIndex, vertIndex + width + 1, cell.Color);
                    }
                    vertIndex++;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

            meshCollider.sharedMesh = mesh;

            var renderer = GetComponent<MeshRenderer>();
            renderer.sharedMaterial.mainTexture = TextureCreator.CreateTextureFromColorMap(width, height, colors);
        }
    }

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = $"Mesh {name}";
        meshCollider = gameObject.AddComponent<MeshCollider>();
    }

    #endregion Mesh

    public List<Cell> Cells;
    public Chunk Data;

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
        //using (var sw = new Instrumenter(nameof(ChunkRenderer)))
        {
            transform.position = new Vector3(Data.X * Game.Instance.Map.ChunkSize, Data.Y * Game.Instance.Map.ChunkSize);
            //DrawGround();
            Populate();
            UpdateInterlocked();
        }

        Triangulate();
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

    private void UpdateInterlocked()
    {
        foreach (var wall in Cells.Where(c => c.Structure?.IsInterlocking() == true).Select(c => c.Structure))
        {
            wall.UpdateInterlocking();
        }
    }
}