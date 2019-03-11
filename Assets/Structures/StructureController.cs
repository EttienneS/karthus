using System.Collections.Generic;
using UnityEngine;

public class StructureController : MonoBehaviour
{
    internal static Color BluePrintColor = new Color(0.3f, 1f, 1f, 0.4f);
    internal static Color StructureColor = new Color(0.6f, 0.6f, 0.6f);

    public Dictionary<int, StructureData> StructureIdLookup = new Dictionary<int, StructureData>();
    public Dictionary<StructureData, Structure> StructureLookup = new Dictionary<StructureData, Structure>();
    public Structure structurePrefab;
    internal Dictionary<string, StructureData> StructureDataReference = new Dictionary<string, StructureData>();

    private Dictionary<string, string> _structureTypeFileMap;

    internal Dictionary<string, string> StructureTypeFileMap
    {
        get
        {
            if (_structureTypeFileMap == null)
            {
                _structureTypeFileMap = new Dictionary<string, string>();
                foreach (var structureFile in FileController.Instance.StructureJson)
                {
                    var data = StructureData.GetFromJson(structureFile.text);
                    StructureTypeFileMap.Add(data.Name, structureFile.text);
                    StructureDataReference.Add(data.Name, data);
                }
            }
            return _structureTypeFileMap;
        }
    }

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

        structure.Data.SetBlueprintState(false);

        if (!string.IsNullOrEmpty(structure.Data.Layer))
        {
            structure.SpriteRenderer.sortingLayerName = structure.Data.Layer;
        }

        return structure;
    }

    internal Structure GetStructureBluePrint(string name)
    {
        var structure = GetStructure(name);
        structure.Data.SetBlueprintState(true);
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
        MapGrid.Instance.GetCellAtCoordinate(structure.Data.Coordinates).Structure = null;

        Destroy(structure.gameObject);
    }

    internal Structure LoadStructure(StructureData savedStructure)
    {
        var structure = Instantiate(structurePrefab, transform);
        structure.Data = savedStructure;
        IndexStructure(structure);

        structure.LoadSprite();
        structure.Data.SetBlueprintState(structure.Data.IsBluePrint);

        return structure;
    }

    private void IndexStructure(Structure structure)
    {
        StructureLookup.Add(structure.Data, structure);
        StructureIdLookup.Add(structure.Data.Id, structure.Data);

        structure.name = $"{structure.Data.Name} ({structure.Data.Id})";
    }
}