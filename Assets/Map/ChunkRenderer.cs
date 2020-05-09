using Assets.Helpers;
using Assets.Sprites;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    public Chunk Data;
    public MeshRenderer MeshRenderer;
    private Mesh mesh;
    private MeshCollider meshCollider;

    private int triangleIndex;
    private int[] triangles;
    private Vector2[] uvs;
    private Vector3[] vertices;
    public int MeshVertexWidth => Game.Instance.ChunkSize + 1;

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;

        triangleIndex += 3;
    }

    public Color GetColor(Cell cell)
    {
        if (cell == null)
        {
            return new Color(0, 0, 0, 0);
        }

        return ColorExtensions.GetColorFromHex(cell.BiomeRegion.Color);
    }

    public void Start()
    {
        Triangulate();
        UpdateTexture();

        using (Instrumenter.Start())
        {
            transform.position = new Vector3(Data.X * Game.Instance.ChunkSize, Data.Y * Game.Instance.ChunkSize);
            Populate(Game.Instance.Map.GetRectangle(Data.X * Game.Instance.ChunkSize,
                                                    Data.Y * Game.Instance.ChunkSize,
                                                    Game.Instance.ChunkSize,
                                                    Game.Instance.ChunkSize));
        }
        var waterSize = 50;
        var waterLevel = 1.5f;

        for (int y = 0; y <= Game.Instance.ChunkSize / waterSize; y++)
        {
            for (int x = 0; x <= Game.Instance.ChunkSize / waterSize; x++)
            {
                var water = Instantiate(Game.Instance.Map.WaterPrefab, transform);
                water.transform.position = new Vector3(x * waterSize,  waterLevel, y * waterSize);
            }
        }
    }

    public void Triangulate()
    {
        using (Instrumenter.Start())
        {
            mesh.Clear();

            var maxMeshVertexes = MeshVertexWidth - 1;
            uvs = new Vector2[MeshVertexWidth * MeshVertexWidth];
            vertices = new Vector3[MeshVertexWidth * MeshVertexWidth];
            triangles = new int[maxMeshVertexes * maxMeshVertexes * 6];

            var vertIndex = 0;
            var height = 0f;

            for (var y = 0; y < MeshVertexWidth; y++)
            {
                for (var x = 0; x < MeshVertexWidth; x++)
                {
                    var cell = Game.Instance.Map.GetCellAtCoordinate(x + (Data.X * MeshVertexWidth), y + (Data.Y * MeshVertexWidth));
                    if (cell != null)
                    {
                        height = cell.Y;
                    }

                    vertices[vertIndex] = new Vector3(x, height, y);
                    uvs[vertIndex] = new Vector2(x / (float)MeshVertexWidth, y / (float)MeshVertexWidth);
                    if (x < maxMeshVertexes && y < maxMeshVertexes)
                    {
                        AddTriangle(vertIndex + MeshVertexWidth, vertIndex + MeshVertexWidth + 1, vertIndex);
                        AddTriangle(vertIndex + 1, vertIndex, vertIndex + MeshVertexWidth + 1);
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
        var colors = new Color[MeshVertexWidth, MeshVertexWidth];
        for (int y = 0; y < MeshVertexWidth; y++)
        {
            for (int x = 0; x < MeshVertexWidth; x++)
            {
                var cell = Game.Instance.Map.GetCellAtCoordinate(x + (Data.X * MeshVertexWidth), y + (Data.Y * MeshVertexWidth));
                colors[x, y] = GetColor(cell);
            }
        }

        var mats = MeshRenderer.materials;
        mats[0].mainTexture = TextureCreator.CreateTextureFromColorMap(MeshVertexWidth, MeshVertexWidth, colors);
        MeshRenderer.materials = mats;
    }

    internal void Populate(List<Cell> cells)
    {
        cells.ForEach(c => c.Populate());
        Data.Populated = true;
    }

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = $"Mesh {name}";
        meshCollider = gameObject.AddComponent<MeshCollider>();
    }
}