using System.Collections.Generic;
using UnityEngine;

public class FileController : MonoBehaviour
{
    private static FileController _instance;

    internal TextAsset[] StructureJson;
    internal TextAsset[] ItemJson;

    public Dictionary<string, TextAsset> ItemLookup;

    public string ItemFolder = "Items";
    public string StructureFolder = "Structures";

    public static FileController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("FileController").GetComponent<FileController>();

                _instance.ItemJson = Resources.LoadAll<TextAsset>(_instance.ItemFolder);
                _instance.StructureJson = Resources.LoadAll<TextAsset>(_instance.StructureFolder);

                _instance.ItemLookup = new Dictionary<string, TextAsset>();

                foreach (var file in _instance.ItemJson)
                {
                    _instance.ItemLookup.Add(file.name, file);
                }
            }

            return _instance;
        }
    }
}