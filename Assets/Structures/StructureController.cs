using System.Collections.Generic;
using System.Linq;
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
                    var data = Structure.GetFromJson(structureFile.text, structureFile.name);
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
            if (structure.Material != "rune")
            {
                DefaultStructureMap.SetTile(coords, null);
                DefaultStructureMap.SetTile(coords, structure.Tile);
            }
            else
            {
                RuneMap.SetTile(coords, null);
                RuneMap.SetTile(coords, structure.Tile);
            }
        }
    }

    public void ClearStructure(Cell cell)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        DefaultStructureMap.SetTile(new Vector3Int(cell.X, cell.Y, 0), tile);
        RuneMap.SetTile(new Vector3Int(cell.X, cell.Y, 0), tile);
    }

    public Structure GetStructure(string name, Faction faction)
    {
        string structureData = StructureTypeFileMap[name];

        Structure structure = Structure.GetFromJson(structureData, name);
        IndexStructure(structure);

        structure.SetBluePrintState(false);
        structure.ManaPool = structure.ManaValue.ToManaPool(structure);

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
            Game.Map.Unbind(structure);

            if (structure.Cell.Floor == structure)
            {
                structure.Cell.Floor = null;
            }
            else
            {
                structure.Cell.Structure = null;
            }

            ClearStructure(structure.Cell);

            if (structure.Spell != null)
            {
                Game.MagicController.FreeRune(structure);
            }
            IdService.RemoveEntity(structure);
            FactionController.Factions[structure.FactionName].Structures.Remove(structure);
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

        var nonRunes = structures.Where(s => s.Material != "rune");
        var tiles = structures.Select(c => c.Tile).ToArray();
        var coords = structures.Select(c => c.Cell.ToVector3Int()).ToArray();
        Game.StructureController.DefaultStructureMap.SetTiles(coords, tiles);

        var runes = structures.Except(nonRunes);
        tiles = runes.Select(c => c.Tile).ToArray();
        coords = runes.Select(c => c.Cell.ToVector3Int()).ToArray();
        Game.StructureController.RuneMap.SetTiles(coords, tiles);
    }
}