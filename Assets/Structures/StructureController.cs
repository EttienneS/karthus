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
        var structures = new List<Structure>();
        var floors = new List<Structure>(); 
        foreach (var cell in cells)
        {
            if (IdService.StructureCellLookup.ContainsKey(cell))
            {
                foreach (var s in IdService.StructureCellLookup[cell])
                {
                    if (s.IsFloor())
                    {
                        floors.Add(s);
                    } 
                    else
                    {
                        structures.Add(s);
                    }
                }
            }
        }

        var tiles = structures.Select(c => c.Tile).ToArray();
        var coords = structures.Select(c => c.Cell.ToVector3Int()).ToArray();
        Game.StructureController.DefaultStructureMap.SetTiles(coords, tiles);

        var floorTiles = floors.Select(c => c.Tile).ToArray();
        var floorCoords = floors.Select(c => c.Cell.ToVector3Int()).ToArray();
        Game.StructureController.DefaultFloorMap.SetTiles(floorCoords, floorTiles);
    }

    public Structure SpawnStructure(string name, Cell cell, Faction faction)
    {
        string structureData = StructureTypeFileMap[name];

        Structure structure = Structure.GetFromJson(structureData);
        structure.Cell = cell;
        IndexStructure(structure);

        structure.SetBluePrintState(false);

        faction?.AddStructure(structure);

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
            IdService.RemoveEntity(structure);
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
        IdService.EnrollEntity(structure);
    }
}