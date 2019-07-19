using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StructureController : MonoBehaviour
{
    public Dictionary<int, Structure> StructureIdLookup = new Dictionary<int, Structure>();
    internal Dictionary<string, Structure> StructureDataReference = new Dictionary<string, Structure>();
    private Tilemap _tilemap;

    internal Tilemap Tilemap
    {
        get
        {
            if (_tilemap == null)
            {
                _tilemap = GetComponentInChildren<Tilemap>();
            }

            return _tilemap;
        }
    }

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
                    var data = Structure.GetFromJson(structureFile.text);
                    StructureTypeFileMap.Add(data.Name, structureFile.text);
                    StructureDataReference.Add(data.Name, data);
                }
            }
            return _structureTypeFileMap;
        }
    }

    public void RefreshStructure(Structure structure)
    {
        if (structure.Coordinates == null)
        {
            return;
        }

        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = Game.SpriteStore.GetSpriteByName(structure.SpriteName);

        if (structure.IsBluePrint)
        {
            tile.color = ColorConstants.BluePrintColor;
        }
        else
        {
            if (Game.MapGrid.Cells != null)
            {
                tile.color = Game.MapGrid.GetCellAtCoordinate(structure.Coordinates).Bound ?
                    ColorConstants.BaseColor :
                    ColorConstants.UnboundColor;
            }
        }

        Tilemap.SetTile(new Vector3Int(structure.Coordinates.X, structure.Coordinates.Y, 0), tile);
    }

    public void ClearStructure(Coordinates coordinates)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        Tilemap.SetTile(new Vector3Int(coordinates.X, coordinates.Y, 0), tile);
    }

    public Structure GetStructure(string name, Faction faction)
    {
        string structureData = StructureTypeFileMap[name];

        Structure structure = Structure.GetFromJson(structureData);
        structure.Id = IdService.UniqueId();

        IndexStructure(structure);

        structure.SetBluePrintState(false);
        structure.ManaPool = structure.ManaValue.ToManaPool();

        if (faction != null)
        {
            faction.AddStructure(structure);
        }

        return structure;
    }

    internal void DestroyStructure(Structure structure)
    {
        if (structure != null)
        {
            Game.MapGrid.GetCellAtCoordinate(structure.Coordinates).Structure = null;
            Game.MapGrid.Unbind(structure.GetGameId());
            ClearStructure(structure.Coordinates);

            if (structure.Spell != null)
            {
                Game.MagicController.FreeRune(structure);
            }

            StructureIdLookup.Remove(structure.Id);
        }
    }

    internal Structure GetStructureBluePrint(string name, Faction faction)
    {
        var structure = GetStructure(name, faction);
        structure.SetBluePrintState(true);
        return structure;
    }

    private void IndexStructure(Structure structure)
    {
        StructureIdLookup.Add(structure.Id, structure);
        if (structure.Spell != null)
        {
            Game.MagicController.AddRune(structure);
        }
    }
}