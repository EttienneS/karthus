using System.Collections.Generic;
using UnityEngine;

public class FileController : MonoBehaviour
{

    internal TextAsset[] StructureJson;
    internal TextAsset[] ItemJson;

    public Dictionary<string, TextAsset> ItemLookup;

    public string ItemFolder = "Items";
    public string StructureFolder = "Structures";

    

    public void Load()
    {
        ItemJson = Resources.LoadAll<TextAsset>(ItemFolder);
        StructureJson = Resources.LoadAll<TextAsset>(StructureFolder);

        ItemLookup = new Dictionary<string, TextAsset>();

        foreach (var file in ItemJson)
        {
            ItemLookup.Add(file.name, file);
        }
    }
}