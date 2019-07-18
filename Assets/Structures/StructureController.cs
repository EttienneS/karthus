using System;
using System.Collections.Generic;
using UnityEngine;

public class StructureController : MonoBehaviour
{
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
                foreach (var structureFile in Game.FileController.StructureJson)
                {
                    var data = StructureData.GetFromJson(structureFile.text);
                    StructureTypeFileMap.Add(data.Name, structureFile.text);
                    StructureDataReference.Add(data.Name, data);
                }
            }
            return _structureTypeFileMap;
        }
    }

    public Sprite GetSpriteForStructure(string structureName)
    {
        return Game.SpriteStore.GetSpriteByName(StructureDataReference[structureName].SpriteName);
    }

    public Structure GetStructure(string name, Faction faction)
    {
        var structure = Instantiate(structurePrefab, transform);

        string structureData = StructureTypeFileMap[name];

        structure.Load(structureData);
        structure.Data.Id = IdService.UniqueId();

        if (!string.IsNullOrEmpty(structure.Data.Material))
        {
            structure.SpriteRenderer.material = Game.MaterialController.GetMaterial(structure.Data.Material);
            structure.SpriteRenderer.color = new Color(0.2f, 0.2f, 0.2f);
        }

        IndexStructure(structure);

        structure.Data.SetBlueprintState(false);
        structure.Data.ManaPool = structure.Data.ManaValue.ToManaPool();

        if (!string.IsNullOrEmpty(structure.Data.Layer))
            structure.SpriteRenderer.sortingLayerName = structure.Data.Layer;

        if (faction != null)
            faction.AddStructure(structure.Data);

        return structure;
    }

    internal void DestroyStructure(StructureData structure)
    {
        Game.MapGrid.Unbind(structure.GetGameId());
        DestroyStructure(structure.LinkedGameObject);
    }


    internal void DestroyStructure(Structure structure)
    {
        if (structure != null)
        {
            StructureLookup.Remove(structure.Data);
            StructureIdLookup.Remove(structure.Data.Id);

            Game.MapGrid.GetCellAtCoordinate(structure.Data.Coordinates).Structure = null;
            Game.Controller.AddItemToDestroy(structure.gameObject);
        }
    }

    internal Structure GetStructureBluePrint(string name, Faction faction)
    {
        var structure = GetStructure(name, faction);
        structure.Data.SetBlueprintState(true);
        return structure;
    }

    internal Structure GetStructureForData(StructureData structureData)
    {
        return StructureLookup[structureData];
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