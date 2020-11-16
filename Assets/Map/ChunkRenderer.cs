using Assets.Helpers;
using Assets.Map;
using Assets.ServiceLocator;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    public Chunk Data;
    public MeshRenderer MeshRenderer;

    private List<Color> _colors;
    private Mesh _mesh;
    private MeshCollider _meshCollider;
    private List<int> _triangles;
    private List<Vector2> _uvs;
    private List<Vector3> _vertices;

    public void AddTriangle(int a, int b, int c)
    {
        _triangles.Add(a);
        _triangles.Add(b);
        _triangles.Add(c);
    }

    public void CreateMesh(ChunkCell[,] cells)
    {
        using (Instrumenter.Start())
        {
            GenerateMesh(cells);
        }
    }

    public void Start()
    {
        transform.position = GetPosition();

        CreateMesh(Data.ChunkCells);

        if (MapGenerationData.Instance.CreateWater)
        {
            AddWaterLevel();
        }
    }

    private Vector3 GetPosition()
    {
        return new Vector3(Data.X * MapGenerationData.Instance.ChunkSize + 0.5f, 0, Data.Z * MapGenerationData.Instance.ChunkSize + 0.5f);
    }

    private void AddVert(float x, float y, float z, Color color)
    {
        _vertices.Add(new Vector3(x, y, z));
        _uvs.Add(new Vector2(x / GetWidth(), z / GetWidth()));
        _colors.Add(color);
    }
    public GameObject WaterPrefab;

    private void AddWaterLevel()
    {
        var waterSize = WaterPrefab.transform.localScale.x * 10;
        var offset = (waterSize / 2) - 0.5f;
        var waterLevel = 0 - MapGenerationData.Instance.WaterLevel;

        for (int y = 0; y < MapGenerationData.Instance.ChunkSize / waterSize; y++)
        {
            for (int x = 0; x < MapGenerationData.Instance.ChunkSize / waterSize; x++)
            {
                var water = Instantiate(WaterPrefab, transform);
                water.transform.localPosition = new Vector3((x * waterSize) + offset, waterLevel, (y * waterSize) + offset);
            }
        }
    }

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
        _mesh.name = $"Mesh {name}";
        _meshCollider = gameObject.AddComponent<MeshCollider>();
    }

    private void GenerateMesh(ChunkCell[,] cells)
    {
        _mesh.Clear();

        _uvs = new List<Vector2>();
        _vertices = new List<Vector3>();
        _colors = new List<Color>();
        _triangles = new List<int>();

        var i = 0;
        const int vertsPerCell = 4;

        var width = GetWidth();
        for (var z = 0; z < width; z++)
        {
            for (var x = 0; x < width; x++)
            {
                var cell = cells[x, z];

                var height = cell.Y;
                AddVert(x - 0.5f, height, z - 0.5f, cell.Color);
                AddVert(x + 0.5f, height, z - 0.5f, cell.Color);
                AddVert(x + 0.5f, height, z + 0.5f, cell.Color);
                AddVert(x - 0.5f, height, z + 0.5f, cell.Color);

                var c = i * vertsPerCell;
                AddTriangle(c + 3, c + 2, c + 1);
                AddTriangle(c + 1, c, c + 3);

                if (x < width - 1)
                {
                    AddTriangle(c + 1, c + 2, c + 4);
                    AddTriangle(c + 4, c + 2, c + 7);
                }

                var cz = width * 4;
                if (z < width - 1)
                {
                    AddTriangle(c + 2, c + 3, c + cz + 1);
                    AddTriangle(c + cz + 1, c + 3, c + cz);
                }

                i++;
            }
        }

        _mesh.vertices = _vertices.ToArray();
        _mesh.colors = _colors.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.uv = _uvs.ToArray();
        _mesh.RecalculateNormals();
        _meshCollider.sharedMesh = _mesh;
    }

    private int GetWidth()
    {
        return MapGenerationData.Instance.ChunkSize;
    }
}