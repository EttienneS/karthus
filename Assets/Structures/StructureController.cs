﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StructureController : MonoBehaviour
{
    public Tilemap DefaultFloorMap;
    public Tilemap DefaultStructureMap;
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
            DefaultFloorMap.SetTile(new Vector3Int(structure.Cell.X, structure.Cell.Y, 0), tile);
        }
        else
        {
            DefaultStructureMap.SetTile(new Vector3Int(structure.Cell.X, structure.Cell.Y, 0), tile);
        }
    }

    public void DrawStructures(IEnumerable<Cell> cells)
    {
        var structures = cells
                            .Where(c => c.Structure != null)
                            .Select(c => c.Structure)
                            .ToList();

        var floors = cells
                            .Where(c => c.Floor != null)
                            .Select(c => c.Floor)
                            .ToList();

        if (structures.Count > 0)
        {
            var tiles = structures.Select(c => c.Tile).ToArray();
            var coords = structures.Select(c => c.Cell.ToVector3Int()).ToArray();
            Game.StructureController.DefaultStructureMap.SetTiles(coords, tiles);
        }

        if (floors.Count > 0)
        {
            var floorTiles = floors.Select(c => c.Tile).ToArray();
            var floorCoords = floors.Select(c => c.Cell.ToVector3Int()).ToArray();
            Game.StructureController.DefaultFloorMap.SetTiles(floorCoords, floorTiles);
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
        var coords = new Vector3Int(structure.Cell.X, structure.Cell.Y, 0);
        if (structure.IsFloor())
        {
            DefaultFloorMap.SetTile(coords, null);
            DefaultFloorMap.SetTile(coords, structure.Tile);
        }
        else
        {
            DefaultStructureMap.SetTile(coords, null);
            DefaultStructureMap.SetTile(coords, structure.Tile);
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