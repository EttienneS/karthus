using Assets.Helpers;
using Assets.Sprites;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

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
            return new Color(0,0,0,0);
        }

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
    }

    public void Triangulate()
    {
        using (Instrumenter.Start())
        {
            mesh.Clear();

            uvs = new Vector2[MeshVertexWidth * MeshVertexWidth];
            vertices = new Vector3[MeshVertexWidth * MeshVertexWidth];
            triangles = new int[(MeshVertexWidth - 1) * (MeshVertexWidth - 1) * 6];

            var vertIndex = 0;
            var height = 0f;
            for (int y = 0; y < MeshVertexWidth; y++)
            {
                for (int x = 0; x < MeshVertexWidth; x++)
                {
                    var cell = Game.Instance.Map.GetCellAtCoordinate(x + (Data.X * MeshVertexWidth), y + (Data.Y * MeshVertexWidth));
                    if (cell != null)
                    {
                        height = cell.RenderHeight;
                    }

                    vertices[vertIndex] = new Vector3(x, y, height);
                    uvs[vertIndex] = new Vector2(x / (float)MeshVertexWidth, y / (float)MeshVertexWidth);
                    if (x < MeshVertexWidth - 1 && y < MeshVertexWidth - 1)
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