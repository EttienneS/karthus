using Assets.Helpers;
using Assets.Map;
using Assets.ServiceLocator;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    public Chunk Data;
    public MeshRenderer MeshRenderer;

    public int w => MapGenerationData.Instance.ChunkSize;

    private Mesh mesh;
    private MeshCollider meshCollider;

    private List<int> triangles;
    private List<Vector2> uvs;
    private List<Vector3> vertices;
    private List<Color> colors;

    public void AddTriangle(int a, int b, int c)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);
    }

    public void CreateMesh()
    {
        using (Instrumenter.Start())
        {
            GenerateMesh();
        }
    }

    private void GenerateMesh()
    {
        mesh.Clear();

        uvs = new List<Vector2>();
        vertices = new List<Vector3>();
        colors = new List<Color>();
        triangles = new List<int>();

        var map = Loc.GetMap();
        var i = 0;
        const int vertsPerCell = 4;
        for (var z = 0; z < w; z++)
        {
            for (var x = 0; x < w; x++)
            {
                var cell = map.GetCellAtCoordinate(x, z);

                AddVert(cell, -0.5f, 0, -0.5f);
                AddVert(cell, 0.5f, 0, -0.5f);
                AddVert(cell, 0.5f, 0, 0.5f);
                AddVert(cell, -0.5f, 0, 0.5f);

                var c = i * vertsPerCell;
                AddTriangle(c + 3, c + 2, c + 1);
                AddTriangle(c + 1, c, c + 3);

                if (x < w - 1)
                {
                    AddTriangle(c + 1, c + 2, c + 4);
                    AddTriangle(c + 4, c + 2, c + 7);
                }

                var cz = w * 4;
                if (z < w - 1)
                {
                    AddTriangle(c + 2, c + 3, c + cz + 1);
                    AddTriangle(c + cz + 1, c + 3, c + cz);
                }

                i++;
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    private void AddVert(Cell cell, float offSetX, float offSetY, float offsetZ)
    {
        vertices.Add(new Vector3(cell.X + offSetX, cell.Y + offSetY, cell.Z + offsetZ));
        colors.Add(GetColor(cell));
        uvs.Add(new Vector2(cell.X / (float)w, cell.Z / (float)w));
    }

    public Color GetColor(Cell cell)
    {
        if (cell == null)
        {
            return new Color(0, 0, 0, 0);
        }

        var biomes = cell.NonNullNeighbors.Select(c => c.BiomeRegion).ToList();
        biomes.Add(cell.BiomeRegion);

        var red = biomes.Average(b => b.Red);
        var green = biomes.Average(b => b.Green);
        var blue = biomes.Average(b => b.Blue);
        var alpha = biomes.Average(b => b.Alpha);

        return new Color(red, green, blue, alpha);
    }

    public void Start()
    {
        transform.position = new Vector3(Data.X * MapGenerationData.Instance.ChunkSize + 0.5f, 0, Data.Z * MapGenerationData.Instance.ChunkSize + 0.5f);

        CreateMesh();

        if (MapGenerationData.Instance.CreateWater)
        {
            AddWaterLevel();
        }
    }

    private void AddWaterLevel()
    {
        using (Instrumenter.Start())
        {
            var waterSize = Loc.GetMap().WaterPrefab.transform.localScale.x * 10;
            var offset = (waterSize / 2) - 0.5f;
            var waterLevel = 0 - MapGenerationData.Instance.WaterLevel;

            for (int y = 0; y < MapGenerationData.Instance.ChunkSize / waterSize; y++)
            {
                for (int x = 0; x < MapGenerationData.Instance.ChunkSize / waterSize; x++)
                {
                    var water = Instantiate(Loc.GetMap().WaterPrefab, transform);
                    water.transform.localPosition = new Vector3((x * waterSize) + offset, waterLevel, (y * waterSize) + offset);
                }
            }
        }
    }

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = $"Mesh {name}";
        meshCollider = gameObject.AddComponent<MeshCollider>();
    }
}