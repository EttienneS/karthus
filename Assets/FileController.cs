using System.Collections.Generic;
using UnityEngine;

public class FileController : MonoBehaviour
{
    internal TextAsset[] StructureJson;
    internal TextAsset[] ConstructFiles;
    internal TextAsset[] CreatureFiles;

    public Dictionary<string, TextAsset> ItemLookup;

    public string StructureFolder = "Structures";
    public string ConstructFolder = "Constructs";
    public string CreatureFolder = "Creatures";

    public void Load()
    {
        StructureJson = Resources.LoadAll<TextAsset>(StructureFolder);
        ConstructFiles = Resources.LoadAll<TextAsset>(ConstructFolder);
        CreatureFiles = Resources.LoadAll<TextAsset>(CreatureFolder);
    }
}