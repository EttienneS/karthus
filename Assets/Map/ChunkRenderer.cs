using Assets.Helpers;
using Assets.Sprites;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkRenderer : MonoBehaviour
{
    public Chunk Data;
    public MeshRenderer MeshRenderer;
    public int MeshVertexWidth => Game.MapGenerationData.ChunkSize + 1;

    private Mesh mesh;
    private MeshCollider meshCollider;

    public WorldCanvasController WorldCanvasController;

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
            mesh.Clear();

            var maxMeshVertexes = MeshVertexWidth - 1;
            uvs = new Vector2[MeshVertexWidth * MeshVertexWidth];
            vertices = new Vector3[MeshVertexWidth * MeshVertexWidth];
            colors = new Color[MeshVertexWidth * MeshVertexWidth];
            triangles = new int[maxMeshVertexes * maxMeshVertexes * 6];

            var vertIndex = 0;
            var height = 0f;

            for (var y = 0; y < MeshVertexWidth; y++)
            {
                for (var x = 0; x < MeshVertexWidth; x++)
                {
                    var cell = Map.Instance.GetCellAtCoordinate(x + (Data.X * MeshVertexWidth), y + (Data.Z * MeshVertexWidth));
                    if (cell != null)
                    {
                        height = cell.Y;
                    }

                    colors[vertIndex] = GetColor(cell);
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
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            meshCollider.sharedMesh = mesh;
        }
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
        transform.position = new Vector3(Data.X * Game.MapGenerationData.ChunkSize, 0, Data.Z * Game.MapGenerationData.ChunkSize);

        CreateMesh();

        if (Game.MapGenerationData.CreateWater)
        {
            AddWaterLevel();
        }
    }

    public void UpdateTexture()
    {
        var scale = 10;
        var colors = new Color[MeshVertexWidth * scale, MeshVertexWidth * scale];
        for (int y = 0; y < MeshVertexWidth * scale; y++)
        {
            for (int x = 0; x < MeshVertexWidth * scale; x++)
            {
                var cell = Map.Instance.GetCellAtCoordinate(Mathf.Floor((x + (Data.X * MeshVertexWidth)) / scale),
                                                                 Mathf.Floor((y + (Data.Z * MeshVertexWidth)) / scale));
                colors[x, y] = GetColor(cell);
            }
        }

        var mats = MeshRenderer.materials;
        mats[0].mainTexture = TextureCreator.CreateTextureFromColorMap(MeshVertexWidth * scale, MeshVertexWidth * scale, colors);

        MeshRenderer.materials = mats;
    }

    private void AddWaterLevel()
    {
        using (Instrumenter.Start())
        {
            var waterSize = Map.Instance.WaterPrefab.transform.localScale.x * 10;
            var offset = waterSize / 2;
            var waterLevel = 0 - Game.MapGenerationData.WaterLevel;

            for (int y = 0; y < Game.MapGenerationData.ChunkSize / waterSize; y++)
            {
                for (int x = 0; x < Game.MapGenerationData.ChunkSize / waterSize; x++)
                {
                    var water = Instantiate(Map.Instance.WaterPrefab, transform);
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