using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StructureController : MonoBehaviour
{
    internal Dictionary<string, Structure> StructureDataReference = new Dictionary<string, Structure>();


    public Tilemap DefaultStructureMap;

    public Tilemap RuneMap;


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
        if (structure.Material != "rune")
        {
            DefaultStructureMap.SetTile(new Vector3Int(structure.Coordinates.X, structure.Coordinates.Y, 0), tile);
        }
        else
        {
            RuneMap.SetTile(new Vector3Int(structure.Coordinates.X, structure.Coordinates.Y, 0), tile);
        }
    }

    public void ClearStructure(Coordinates coordinates)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        DefaultStructureMap.SetTile(new Vector3Int(coordinates.X, coordinates.Y, 0), tile);
        RuneMap.SetTile(new Vector3Int(coordinates.X, coordinates.Y, 0), tile);
    }

    public Structure GetStructure(string name, Faction faction)
    {
        string structureData = StructureTypeFileMap[name];

        Structure structure = Structure.GetFromJson(structureData);
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
            Game.MapGrid.Unbind(structure.Id);
            ClearStructure(structure.Coordinates);

            if (structure.Spell != null)
            {
                Game.MagicController.FreeRune(structure);
            }
            IdService.RemoveEntity(structure);
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
        IdService.EnrollEntity(structure);
        if (structure.Spell != null)
        {
            Game.MagicController.AddRune(structure);
        }
    }
}