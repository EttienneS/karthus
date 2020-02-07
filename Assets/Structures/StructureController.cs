using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StructureController : MonoBehaviour
{
    internal Dictionary<string, Structure> StructureDataReference = new Dictionary<string, Structure>();
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

    public void ClearStructure(Structure structure)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();

        if (structure.IsFloor())
        {
            Game.Map.SetTile(structure.Cell, tile, Map.TileLayer.Floor);
        }
        else
        {
            Game.Map.SetTile(structure.Cell, tile, Map.TileLayer.Structure);
        }
    }

    public Structure SpawnStructure(string name, Cell cell, Faction faction)
    {
        var structureData = StructureTypeFileMap[name];

        var structure = Structure.GetFromJson(structureData);
        structure.Cell = cell;
        IndexStructure(structure);

        structure.SetBluePrintState(false);
        faction?.AddStructure(structure);

        if (structure is Container container)
        {
            var zone = Game.ZoneController.GetZoneForCell(cell);

            if (zone != null && zone is StorageZone store)
            {
                container.Filter = store.Filter;
            }
        }

        return structure;
    }

    public void RefreshStructure(Structure structure)
    {
        if (structure.Cell == null)
        {
            return;
        }
        if (structure.IsFloor())
        {
            Game.Map.SetTile(structure.Cell, structure.Tile, Map.TileLayer.Floor);
        }
        else
        {
            Game.Map.SetTile(structure.Cell, structure.Tile, Map.TileLayer.Structure);
        }
    }

    internal void DestroyStructure(Structure structure)
    {
        if (structure != null)
        {
            if (structure.Cell != null)
            {
                ClearStructure(structure);
            }
            else
            {
                Debug.Log("Unbound structure");
            }

            if (structure.AutoInteractions.Count > 0)
            {
                Game.MagicController.RemoveEffector(structure);
            }
            Game.IdService.RemoveEntity(structure);
            Game.FactionController.Factions[structure.FactionName].Structures.Remove(structure);
        }
    }

    internal Structure GetStructureBluePrint(string name, Cell cell, Faction faction)
    {
        var structure = SpawnStructure(name, cell, faction);
        structure.SetBluePrintState(true);
        return structure;
    }

    private void IndexStructure(Structure structure)
    {
        Game.IdService.EnrollEntity(structure);
    }
}