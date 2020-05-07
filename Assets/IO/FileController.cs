using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FileController : MonoBehaviour
{
    internal TextAsset[] StructureJson;
    internal TextAsset[] ConstructFiles;
    internal TextAsset[] CreatureFiles;
    internal TextAsset[] BiomeFiles;
    internal TextAsset[] ItemFiles;

    public MeshRenderer[] Meshes;

    public Dictionary<string, TextAsset> ItemLookup;

    internal Dictionary<string, MeshRenderer> MeshLookup = new Dictionary<string, MeshRenderer>();

    public string StructureFolder = "Structures";
    public string ConstructFolder = "Constructs";
    public string CreatureFolder = "Creatures";
    public string BiomeFolder = "Biomes";
    public string ItemFolder = "Items";

    public void Awake()
    {
        StructureJson = Resources.LoadAll<TextAsset>(StructureFolder);
        ConstructFiles = Resources.LoadAll<TextAsset>(ConstructFolder);
        CreatureFiles = Resources.LoadAll<TextAsset>(CreatureFolder);
        BiomeFiles = Resources.LoadAll<TextAsset>(BiomeFolder);
        ItemFiles = Resources.LoadAll<TextAsset>(ItemFolder);

        foreach (var mesh in Meshes)
        {
            MeshLookup.Add(mesh.name, mesh);
        }
    }

    internal MeshRenderer GetMesh(string name)
    {
        if (MeshLookup.ContainsKey(name))
        {
            return MeshLookup[name];
        }
        return MeshLookup.First().Value;
    }
}