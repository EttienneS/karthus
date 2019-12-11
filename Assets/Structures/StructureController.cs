using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StructureController : MonoBehaviour
{
    internal Dictionary<string, Structure> StructureDataReference = new Dictionary<string, Structure>();

    public Tilemap DefaultStructureMap;

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
        if (structure.Cell == null)
        {
            return;
        }

        if (structure.IsFloor())
        {
            structure.Cell.UpdateTile();
        }
        else
        {
            var coords = new Vector3Int(structure.Cell.X, structure.Cell.Y, 0);
            DefaultStructureMap.SetTile(coords, null);
            DefaultStructureMap.SetTile(coords, structure.Tile);
        }
    }

    public void ClearStructure(Cell cell)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        DefaultStructureMap.SetTile(new Vector3Int(cell.X, cell.Y, 0), tile);
    }

    public Structure GetStructure(string name, Faction faction)
    {
        //if (!StructureTypeFileMap.ContainsKey(name))
        //{
        //    Debug.LogError($"Structure not found: {name}");
        //}

        string structureData = StructureTypeFileMap[name];

        Structure structure = Structure.GetFromJson(structureData);
        IndexStructure(structure);

        structure.SetBluePrintState(false);

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
            if (structure.Cell != null)
            {
                if (structure.Cell.Floor == structure)
                {
                    structure.Cell.Floor = null;
                }
                else
                {
                    structure.Cell.Structure = null;
                }

                ClearStructure(structure.Cell);
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

    internal Structure GetStructureBluePrint(string name, Faction faction)
    {
        var structure = GetStructure(name, faction);
        structure.SetBluePrintState(true);
        return structure;
    }

    private void IndexStructure(Structure structure)
    {
        IdService.EnrollEntity(structure);
    }

    public void DrawAllStructures(IEnumerable<Cell> cells)
    {
        var structures = cells.Where(c => c.Structure != null)
                              .Select(c => c.Structure)
                              .Where(c => c.Cell != null);

        var tiles = structures.Select(c => c.Tile).ToArray();
        var coords = structures.Select(c => c.Cell.ToVector3Int()).ToArray();
        Game.StructureController.DefaultStructureMap.SetTiles(coords, tiles);
    }
}