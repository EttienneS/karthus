using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FileController : MonoBehaviour
{

    private Dictionary<string, string> _files = new Dictionary<string, string>();

    private static FileController _instance;

    public string GetFile(string name)
    {
        return _files[name];
    }

    public static FileController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("FileController").GetComponent<FileController>();
            }

            return _instance;
        }
    }

    public List<string> LoadJsonFilesInFolder(string path)
    {
        var addedFiles = new List<string>();
        foreach (var asset in Resources.LoadAll(path, typeof(TextAsset)))
        {
            var assetPath = AssetDatabase.GetAssetPath(asset);
            assetPath = assetPath.Replace("Assets/Resources/", "");

            var text = GetJsonFileContent(assetPath);
            _files.Add(assetPath, text);
            addedFiles.Add(assetPath);
        }

        return addedFiles;
    }

    public static string GetJsonFileContent(string path)
    {
        var jsonFilePath = path.Replace(".json", "");
        TextAsset loadedJsonFile = Resources.Load<TextAsset>(jsonFilePath);
        return loadedJsonFile.text;
    }
}
