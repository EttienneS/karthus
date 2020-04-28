using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    #region Mesh

    private const float Bridge = 0.1f;
    private const float Core = 1f - (2 * Bridge);
    private List<Color> colors = new List<Color>();
    private Mesh mesh;
    private MeshCollider meshCollider;
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Vector3> vertices = new List<Vector3>();

    public float GetColor(Cell cell)
    {
        switch (cell.BiomeRegion.SpriteName)
        {
            case "Dirt":
                return 0f;

            case "Forest":
                return 0.1f;

            case "Grass":
                return 0.2f;

            case "PatchyGrass":
                return 0.3f;

            case "Sand":
                return 0.4f;

            case "Stone":
                return 0.5f;

            case "Water":
                return 0.6f;

            default:
                throw new KeyNotFoundException(cell.BiomeRegion.SpriteName + " not found");
        }
    }

    public void Triangulate()
    {
        using (Instrumenter.Init())
        {
            mesh.Clear();
            vertices.Clear();
            triangles.Clear();
            colors.Clear();
            for (int i = 0; i < Cells.Count; i++)
            {
                AddCellToMesh(Cells[i]);
            }
            mesh.vertices = vertices.ToArray();
            mesh.colors = colors.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();

            meshCollider.sharedMesh = mesh;
        }
    }

    private void AddCellToMesh(Cell cell)
    {
        var corner = new Vector3(cell.X % Game.Instance.Map.ChunkSize, cell.Y % Game.Instance.Map.ChunkSize);

        var uvOffset = new Vector2(cell.X - (Data.X * Game.Instance.Map.ChunkSize), cell.Y - (Data.Y * Game.Instance.Map.ChunkSize));

        // corner bottom left
        AddQuad(corner, Bridge, Bridge, uvOffset, GetNeighbourColorBlen(cell, Direction.SW, Direction.S, Direction.W));
        // side bottom
        AddQuad(corner + new Vector3(Bridge, 0), Core, Bridge, uvOffset, GetNeighbourColorBlen(cell, Direction.S));
        // corner bottom right
        AddQuad(corner + new Vector3(Bridge + Core, 0), Bridge, Bridge, uvOffset, GetNeighbourColorBlen(cell, Direction.SE, Direction.S, Direction.E));
        // side left
        AddQuad(corner + new Vector3(0, Bridge), Bridge, Core, uvOffset, GetNeighbourColorBlen(cell, Direction.W));
        // corner top left
        AddQuad(corner + new Vector3(0, Bridge + Core), Bridge, Bridge, uvOffset, GetNeighbourColorBlen(cell, Direction.NW, Direction.N, Direction.W));
        // core
        AddQuad(corner + new Vector3(Bridge, Bridge), Core, Core, uvOffset, new Color(0, 0, 0, GetColor(cell)));
        // side top
        AddQuad(corner + new Vector3(Bridge, Bridge + Core), Core, Bridge, uvOffset, GetNeighbourColorBlen(cell, Direction.N));
        // corner top right
        AddQuad(corner + new Vector3(Bridge + Core, Bridge + Core), Bridge, Bridge, uvOffset, GetNeighbourColorBlen(cell, Direction.NE, Direction.N, Direction.E));
        // side right
        AddQuad(corner + new Vector3(Bridge + Core, Bridge), Bridge, Core, uvOffset, GetNeighbourColorBlen(cell, Direction.E));
    }

    private Color GetNeighbourColorBlen(Cell cell, params Direction[] directions)
    {
        var colors = new List<float> { GetColor(cell) };
        foreach (var direction in directions)
        {
            var neighbour = cell.GetNeighbor(direction);
            if (neighbour != null)
            {
                colors.Add(GetColor(neighbour));
            }
        }

        if (colors.Count == 2)
        {
            return new Color(colors[1], 0, 0, colors[0]);
        }
        if (colors.Count == 3)
        {
            return new Color(colors[1], colors[2], 0, colors[0]);
        }
        if (colors.Count == 4)
        {
            return new Color(colors[1], colors[2], colors[3], colors[0]);
        }

        return new Color(0, 0, 0, colors[0]);
    }

    private void AddQuad(Vector3 c, float width, float height, Vector2 uvOffset, Color color)
    {
        AddTriangle(c, c + new Vector3(0, height), c + new Vector3(width, 0), uvOffset, color);
        AddTriangle(c + new Vector3(width, 0), c + new Vector3(0, height), c + new Vector3(width, height), uvOffset, color);
    }

    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uvOffeset, Color color)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        uvs.Add(new Vector2(v1.x - uvOffeset.x, v1.y - uvOffeset.y));
        uvs.Add(new Vector2(v2.x - uvOffeset.x, v2.y - uvOffeset.y));
        uvs.Add(new Vector2(v3.x - uvOffeset.x, v3.y - uvOffeset.y));

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
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

        GetComponent<MeshRenderer>().materials[0].SetTexture("_MainTex", Game.Instance.SpriteStore.MapTextureArray);
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