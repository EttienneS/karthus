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
    public int MeshVertexWidth => Game.Instance.MapData.ChunkSize + 1;

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
        transform.position = new Vector3(Data.X * Game.Instance.MapData.ChunkSize, 0, Data.Z * Game.Instance.MapData.ChunkSize);

        CreateMesh();
        UpdateTexture();

        if (Game.Instance.MapData.Populate)
        {
            using (Instrumenter.Start())
            {
                Populate(Game.Instance.Map.GetRectangle(Data.X * Game.Instance.MapData.ChunkSize,
                                                        Data.Z * Game.Instance.MapData.ChunkSize,
                                                        Game.Instance.MapData.ChunkSize,
                                                        Game.Instance.MapData.ChunkSize));
            }
        }
        if (Game.Instance.MapData.CreateWater)
        {
            AddWaterLevel();
        }
    }

    private void AddWaterLevel()
    {
        using (Instrumenter.Start())
        {
            var waterSize = Game.Instance.Map.WaterPrefab.transform.localScale.x * 10;
            var offset = waterSize / 2;
            var waterLevel = Game.Instance.MapData.WaterLevel;

            for (int y = 0; y < Game.Instance.MapData.ChunkSize / waterSize; y++)
            {
                for (int x = 0; x < Game.Instance.MapData.ChunkSize / waterSize; x++)
                {
                    var water = Instantiate(Game.Instance.Map.WaterPrefab, transform);
                    water.transform.localPosition = new Vector3((x * waterSize) + offset, waterLevel, (y * waterSize) + offset);
                }
            }
        }
    }

    public void CreateMesh()
    {
        using (Instrumenter.Start())
        {
            mesh.Clear();

            var lod = Game.Instance.MapData.LOD * 2;

            var maxMeshVertexes = MeshVertexWidth - 1;
            uvs = new Vector2[MeshVertexWidth * MeshVertexWidth / lod];
            vertices = new Vector3[MeshVertexWidth * MeshVertexWidth / lod];
            triangles = new int[maxMeshVertexes * maxMeshVertexes * 6 / lod];

            var vertIndex = 0;
            var height = 0f;

            var offset = (MeshVertexWidth / lod) + 1;

            for (var z = 0; z < MeshVertexWidth; z += lod)
            {
                for (var x = 0; x < MeshVertexWidth; x += lod)
                {
                    if (!Game.Instance.MapData.Flat)
                    {
                        var cell = Game.Instance.Map.GetCellAtCoordinate(x + (Data.X * MeshVertexWidth), z + (Data.Z * MeshVertexWidth));
                        if (cell != null)
                        {
                            height = cell.Y;
                        }
                    }

                    vertices[vertIndex] = new Vector3(x, height, z);
                    uvs[vertIndex] = new Vector2(x / (float)MeshVertexWidth, z / (float)MeshVertexWidth);
                    if (x < maxMeshVertexes && z < maxMeshVertexes)
                    {
                        AddTriangle(vertIndex + offset, vertIndex + offset + 1, vertIndex);
                        AddTriangle(vertIndex + 1, vertIndex, vertIndex + offset + 1);
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
                var cell = Game.Instance.Map.GetCellAtCoordinate(x + (Data.X * MeshVertexWidth), y + (Data.Z * MeshVertexWidth));
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