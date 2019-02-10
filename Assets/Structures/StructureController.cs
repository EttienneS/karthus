using System.Collections.Generic;
using UnityEngine;

public class StructureController : MonoBehaviour
{
    public Dictionary<StructureData, Structure> StructureLookup = new Dictionary<StructureData, Structure>();
    public Structure structurePrefab;
    internal Dictionary<string, StructureData> StructureDataReference = new Dictionary<string, StructureData>();
    internal Dictionary<string, string> StructureTypeFileMap = new Dictionary<string, string>();
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

    public Sprite GetSpriteForStructure(string structureName)
    {
        return SpriteStore.Instance.GetSpriteByName(StructureDataReference[structureName].SpriteName);
    }

    public Structure GetStructure(string name)
    {
        var structure = Instantiate(structurePrefab, transform);
        structure.Load(StructureTypeFileMap[name]);
        structure.Data.Id = StructureLookup.Keys.Count + 1;

        IndexStructure(structure);

        structure.Data.ToggleBluePrintState(false);

        return structure;
    }

    internal Structure GetStructureBluePrint(string name)
    {
        var structure = GetStructure(name);
        structure.Data.ToggleBluePrintState(true);
        return structure;
    }

    internal Structure GetStructureForData(StructureData structureData)
    {
        return StructureLookup[structureData];
    }
    internal void DestroyStructure(StructureData structure)
    {
        DestroyStructure(structure.LinkedGameObject);
    }

    internal void DestroyStructure(Structure structure)
    {
        MapGrid.Instance.GetCellAtCoordinate(structure.Data.Coordinates).Data.Structure = null;

        Destroy(structure.gameObject);
    }

    private void Start()
    {
        foreach (var structureFile in FileController.Instance.StructureJson)
        {
            var data = StructureData.GetFromJson(structureFile.text);
            StructureTypeFileMap.Add(data.Name, structureFile.text);
            StructureDataReference.Add(data.Name, data);
        }
    }

    internal Structure LoadStructure(StructureData savedStructure)
    {
        var structure = Instantiate(structurePrefab, transform);
        structure.Data = savedStructure;
        IndexStructure(structure);

        structure.LoadSprite();
        structure.Data.ToggleBluePrintState(structure.Data.IsBluePrint);

        return structure;
    }

    private void IndexStructure(Structure structure)
    {
        StructureLookup.Add(structure.Data, structure);

        structure.name = $"{structure.Data.Name} ({structure.Data.Id})";
    }
}