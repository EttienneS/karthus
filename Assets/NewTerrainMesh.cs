using Assets.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewTerrainMesh : MonoBehaviour
{
    public Chunk Data;
    public MeshRenderer MeshRenderer;
    public int MeshVertexWidth => 8;

    private Mesh mesh;
    private MeshCollider meshCollider;

    private int triangleIndex;
    private int[] triangles;
    private Vector2[] uvs;
    private Vector3[] vertices;
    private Color[] colors;

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;

        triangleIndex += 3;
    }

    public void CreateMesh()
    {
        using (Instrumenter.Start())
        {
            if (Style == Mode.Old)
            {
                GenerateMeshOld();
            }
            else
            {
                GenerateMeshNew();
            }
        }
    }
    private void GenerateMeshNew()
    {
        GenerateCells();

        mesh.Clear();
        triangleIndex = 0;
        var maxMeshVertexes = MeshVertexWidth - 1;
        uvs = new Vector2[MeshVertexWidth * MeshVertexWidth];
        vertices = new Vector3[MeshVertexWidth * MeshVertexWidth];
        colors = new Color[MeshVertexWidth * MeshVertexWidth];
        triangles = new int[maxMeshVertexes * maxMeshVertexes * 6];

        var vertIndex = 0;
        var height = 0f;

        for (var z = 0; z < MeshVertexWidth; z++)
        {
            for (var x = 0; x < MeshVertexWidth; x++)
            {
                var cell = GetCellAtCoordinate(x, z);
                if (cell != null)
                {
                    height = cell.Y;
                }

                colors[vertIndex] = cell.Color;
                vertices[vertIndex] = new Vector3(x, height, z);
                uvs[vertIndex] = new Vector2(x / (float)MeshVertexWidth, z / (float)MeshVertexWidth);

                if (x < maxMeshVertexes && z < maxMeshVertexes)
                {
                    AddTriangle(vertIndex + MeshVertexWidth, vertIndex + MeshVertexWidth + 1, vertIndex);
                    AddTriangle(vertIndex + 1, vertIndex, vertIndex + MeshVertexWidth + 1);
                }
                vertIndex++;
            }
        }

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    private void GenerateMeshOld()
    {
        GenerateCells();

        mesh.Clear();
        triangleIndex = 0;
        var maxMeshVertexes = MeshVertexWidth - 1;
        uvs = new Vector2[MeshVertexWidth * MeshVertexWidth];
        vertices = new Vector3[MeshVertexWidth * MeshVertexWidth];
        colors = new Color[MeshVertexWidth * MeshVertexWidth];
        triangles = new int[maxMeshVertexes * maxMeshVertexes * 6];

        var vertIndex = 0;
        var height = 0f;

        for (var z = 0; z < MeshVertexWidth; z++)
        {
            for (var x = 0; x < MeshVertexWidth; x++)
            {
                var cell = GetCellAtCoordinate(x, z);
                if (cell != null)
                {
                    height = cell.Y;
                }

                colors[vertIndex] = cell.Color;
                vertices[vertIndex] = new Vector3(x, height, z);
                uvs[vertIndex] = new Vector2(x / (float)MeshVertexWidth, z / (float)MeshVertexWidth);

                if (x < maxMeshVertexes && z < maxMeshVertexes)
                {
                    AddTriangle(vertIndex + MeshVertexWidth, vertIndex + MeshVertexWidth + 1, vertIndex);
                    AddTriangle(vertIndex + 1, vertIndex, vertIndex + MeshVertexWidth + 1);
                }
                vertIndex++;
            }
        }

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    private List<TestCell> Cells = new List<TestCell>();

    public TestCell GetCellAtCoordinate(float x, float z)
    {
        return Cells.FirstOrDefault(c => c.X == x && c.Z == z);
    }

    public void Start()
    {
        CreateMesh();
    }

    private void GenerateCells()
    {
        Cells = new List<TestCell>();
        for (int x = 0; x < MeshVertexWidth; x++)
        {
            for (int z = 0; z < MeshVertexWidth; z++)
            {
                Cells.Add(new TestCell(x, Random.Range(-1, 1), z, ColorExtensions.GetRandomColor()));
            }
        }
    }

    private float _delta;
    public bool Remake;
    public Mode Style;

    public enum Mode
    {
        Old, New
    }    

    public void Update()
    {
        if (Remake)
        {
            _delta += Time.deltaTime;
            if (_delta > 2.5f)
            {
                _delta = 0;
                CreateMesh();
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

public class TestCell
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public Color Color { get; set; }

    public TestCell(float x, float y, float z, Color color)
    {
        X = x;
        Y = y;
        Z = z;
        Color = color;
    }
}