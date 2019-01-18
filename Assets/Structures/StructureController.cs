using System;
using System.Collections.Generic;
using UnityEngine;

public class StructureController : MonoBehaviour
{
    public Structure structurePrefab;
    internal Dictionary<string, string> StructureTypeFileMap = new Dictionary<string, string>();
    internal Dictionary<string, StructureData> StructureDataReference = new Dictionary<string, StructureData>();
    private static StructureController _instance;
    public static StructureController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("StructureController").GetComponent<StructureController>();
            }

            return _instance;
        }
    }

    internal Structure GetStructureBluePrint(string name)
    {
        var structure = GetStructure(name);
        structure.BluePrint = true;
        return structure;
    }

    private void Start()
    {
        foreach (var structureFile in FileController.Instance.LoadJsonFilesInFolder("Structures"))
        {
            var data = StructureData.GetFromJson(FileController.Instance.GetFile(structureFile));
            StructureTypeFileMap.Add(data.Name, structureFile);
            StructureDataReference.Add(data.Name, data);
        }
    }


    public Sprite GetSpriteForStructure(string structureName)
    {
        return SpriteStore.Instance.GetSpriteByName(StructureDataReference[structureName].SpriteName);
    }


    private Structure GetStructure(string name)
    {
        var structure = Instantiate(structurePrefab, transform);
        structure.Load(FileController.Instance.GetFile(StructureTypeFileMap[name]));
        structure.name = structure.Data.Name;
        return structure;
    }
}
