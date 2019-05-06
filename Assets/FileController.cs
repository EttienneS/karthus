using System.Collections.Generic;
using UnityEngine;

public class FileController : MonoBehaviour
{
    internal TextAsset[] StructureJson;
    internal TextAsset[] ItemJson;
    internal TextAsset[] ConstructFiles;
    internal TextAsset[] CreatureFiles;

    public Dictionary<string, TextAsset> ItemLookup;

    public string ItemFolder = "Items";
    public string StructureFolder = "Structures";
    public string ConstructFolder = "Constructs";
    public string CreatureFolder = "Creatures";

    public void Load()
    {
        ItemJson = Resources.LoadAll<TextAsset>(ItemFolder);
        StructureJson = Resources.LoadAll<TextAsset>(StructureFolder);
        ConstructFiles = Resources.LoadAll<TextAsset>(ConstructFolder);
        CreatureFiles = Resources.LoadAll<TextAsset>(CreatureFolder);

        ItemLookup = new Dictionary<string, TextAsset>();
        foreach (var file in ItemJson)
        {
            ItemLookup.Add(file.name, file);
        }
    }
}