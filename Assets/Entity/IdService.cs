using Assets.Creature;
using Assets.Item;
using Assets.Structures;
using System.Collections.Generic;

public class IdService
{
    public Dictionary<string, CreatureData> CreatureIdLookup = new Dictionary<string, CreatureData>();
    public Dictionary<string, ItemData> ItemIdLookup = new Dictionary<string, ItemData>();
    public Dictionary<string, Structure> StructureIdLookup = new Dictionary<string, Structure>();
    public Dictionary<Cell, List<Structure>> StructureCellLookup = new Dictionary<Cell, List<Structure>>();
    private int _idCounter;

    public void EnrollItem(ItemData item)
    {
        if (string.IsNullOrEmpty(item.Id))
        {
            item.Id = GetId();
        }
        else
        {
            var id = int.Parse(item.Id);
            if (id > _idCounter)
            {
                _idCounter = id + 1;
            }
        }
        ItemIdLookup.Add(item.Id, item);
    }

    public void EnrollCreature(CreatureData creature)
    {
        if (string.IsNullOrEmpty(creature.Id))
        {
            creature.Id = GetId();
        }
        else
        {
            var id = int.Parse(creature.Id);
            if (id > _idCounter)
            {
                _idCounter = id + 1;
            }
        }
        CreatureIdLookup.Add(creature.Id, creature);
    }

    public void EnrollStructure(Structure structure)
    {
        if (string.IsNullOrEmpty(structure.Id))
        {
            structure.Id = GetId();
        }
        else
        {
            var id = int.Parse(structure.Id);
            if (id > _idCounter)
            {
                _idCounter = id + 1;
            }
        }

        StructureIdLookup.Add(structure.Id, structure);

        if (!StructureCellLookup.ContainsKey(structure.Cell))
        {
            StructureCellLookup.Add(structure.Cell, new List<Structure>());
        }
        StructureCellLookup[structure.Cell].Add(structure);
    }

    public string GetId()
    {
        _idCounter++;
        return _idCounter.ToString();
    }

    public bool IsCreature(string id)
    {
        return CreatureIdLookup.ContainsKey(id);
    }

    public bool IsItem(string id)
    {
        return ItemIdLookup.ContainsKey(id);
    }

    public bool IsStructure(string id)
    {
        return StructureIdLookup.ContainsKey(id);
    }

    internal void Clear()
    {
        StructureIdLookup.Clear();
        CreatureIdLookup.Clear();
        ItemIdLookup.Clear();
    }

    internal void RemoveStructure(Structure structure)
    {
        if (StructureIdLookup.ContainsKey(structure.Id))
        {
            StructureIdLookup.Remove(structure.Id);
            StructureCellLookup[structure.Cell].Remove(structure);
        }
    }

    internal void RemoveCreature(CreatureData creature)
    {
        if (CreatureIdLookup.ContainsKey(creature.Id))
        {
            CreatureIdLookup.Remove(creature.Id);
        }
    }

    internal void RemoveItem(ItemData item)
    {
        if (ItemIdLookup.ContainsKey(item.Id))
        {
            ItemIdLookup.Remove(item.Id);
        }
    }

    internal IEnumerable<Structure> GetStructuresInCell(Cell cell)
    {
        if (Game.Instance.IdService.StructureCellLookup.ContainsKey(cell))
        {
            return Game.Instance.IdService.StructureCellLookup[cell];
        }
        return new List<Structure>();
    }
}

public static class IdExtensions
{
    public static CreatureData GetCreature(this string id)
    {
        if (!Game.Instance.IdService.CreatureIdLookup.TryGetValue(id, out var creature))
        {
            return null;
        }
        return creature;
    }

    public static ItemData GetItem(this string id)
    {
        if (!Game.Instance.IdService.ItemIdLookup.TryGetValue(id, out var item))
        {
            return null;
        }
        return item;
    }

    public static Structure GetStructure(this string id)
    {
        if (Game.Instance.IdService.StructureIdLookup.TryGetValue(id, out var structure))
        {
            return structure;
        }
        return null;
    }
}