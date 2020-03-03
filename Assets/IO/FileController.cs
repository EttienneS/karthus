using System.Collections.Generic;
using UnityEngine;

public class FileController : MonoBehaviour
{
    internal TextAsset[] StructureJson;
    internal TextAsset[] ConstructFiles;
    internal TextAsset[] CreatureFiles;
    internal TextAsset[] BiomeFiles;
    internal TextAsset[] ItemFiles;

    public Dictionary<string, TextAsset> ItemLookup;

    public string StructureFolder = "Structures";
    public string ConstructFolder = "Constructs";
    public string CreatureFolder = "Creatures";
    public string BiomeFolder = "Biomes";
    public string ItemFolder = "Items";

    public void Load()
    {
        StructureJson = Resources.LoadAll<TextAsset>(StructureFolder);
        ConstructFiles = Resources.LoadAll<TextAsset>(ConstructFolder);
        CreatureFiles = Resources.LoadAll<TextAsset>(CreatureFolder);
        BiomeFiles = Resources.LoadAll<TextAsset>(BiomeFolder);
        ItemFiles = Resources.LoadAll<TextAsset>(ItemFolder);
    }
}