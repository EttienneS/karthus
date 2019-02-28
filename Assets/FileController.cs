using System.Collections.Generic;
using UnityEngine;

public class FileController : MonoBehaviour
{
    private static FileController _instance;

    public TextAsset[] StructureJson;
    public TextAsset[] ItemJson;

    public Dictionary<string, TextAsset> ItemLookup;

    public static FileController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("FileController").GetComponent<FileController>();
            }

            if (_instance.ItemLookup == null)
            {
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