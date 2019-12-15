using System;
using System.Collections.Generic;
using UnityEngine;

public static class IdService
{
    public static Dictionary<string, Creature> CreatureIdLookup = new Dictionary<string, Creature>();
    public static Dictionary<IEntity, Creature> CreatureLookup = new Dictionary<IEntity, Creature>();
    public static Dictionary<string, Item> ItemIdLookup = new Dictionary<string, Item>();
    public static Dictionary<IEntity, Item> ItemLookup = new Dictionary<IEntity, Item>();
    public static Dictionary<string, Structure> StructureIdLookup = new Dictionary<string, Structure>();
    public static Dictionary<IEntity, Structure> StructureLookup = new Dictionary<IEntity, Structure>();
    public static Dictionary<Cell, List<Structure>> StructureCellLookup = new Dictionary<Cell, List<Structure>>();
    private static int _idCounter;

    public static void EnrollEntity(IEntity entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = _idCounter.ToString();
            _idCounter++;
        }

        if (entity is Structure structure)
        {
            StructureLookup.Add(entity, structure);
            StructureIdLookup.Add(entity.Id, structure);

            if (!StructureCellLookup.ContainsKey(structure.Cell))
            {
                StructureCellLookup.Add(structure.Cell, new List<Structure>());
            }
            StructureCellLookup[structure.Cell].Add(structure);
        }
        else if (entity is Creature creature)
        {
            CreatureLookup.Add(entity, creature);
            CreatureIdLookup.Add(entity.Id, creature);
        }
        else if (entity is Item item)
        {
            ItemLookup.Add(entity, item);
            ItemIdLookup.Add(entity.Id, item);
        }
        else
        {
            throw new NotImplementedException("Unknown entity type!");
        }
    }

    public static Creature GetCreature(this string id)
    {
        if (!CreatureIdLookup.TryGetValue(id, out var creature))
        {
            return null;
        }
        return creature;
    }

    public static Item GetItem(this string id)
    {
        if (!ItemIdLookup.TryGetValue(id, out var item))
        {
            return null;
        }
        return item;
    }

    public static Structure GetStructure(this string id)
    {
        if (StructureIdLookup.TryGetValue(id, out var structure))
        {
            return structure;
        }
        return null;
    }

    public static bool IsCreature(string id)
    {
        return CreatureIdLookup.ContainsKey(id);
    }

    public static bool IsItem(string id)
    {
        return ItemIdLookup.ContainsKey(id);
    }

    public static bool IsStructure(string id)
    {
        return StructureIdLookup.ContainsKey(id);
    }

    internal static void Clear()
    {
        StructureLookup.Clear();
        StructureIdLookup.Clear();

        CreatureLookup.Clear();
        CreatureIdLookup.Clear();

        ItemLookup.Clear();
        ItemIdLookup.Clear();
    }

    internal static void DestroyEntity(IEntity entity)
    {
        if (StructureLookup.ContainsKey(entity))
        {
            Game.StructureController.DestroyStructure(entity as Structure);
        }

        if (CreatureLookup.ContainsKey(entity))
        {
            Game.CreatureController.DestroyCreature((entity as Creature).CreatureRenderer);
        }

        if (ItemLookup.ContainsKey(entity))
        {
            Game.ItemController.DestroyItem(entity as Item);
        }
    }

    internal static IEntity GetEntity(this string id)
    {
        if (IsCreature(id))
        {
            return GetCreature(id);
        }
        if (IsStructure(id))
        {
            return GetStructure(id);
        }
        if (IsItem(id))
        {
            return GetItem(id);
        }

        Debug.LogWarning("Unknown entity type!");
        return null;
    }

    internal static void RemoveEntity(IEntity entity)
    {
        if (StructureLookup.ContainsKey(entity))
        {
            StructureLookup.Remove(entity);
            StructureIdLookup.Remove(entity.Id);

            StructureCellLookup[entity.Cell].Remove(entity as Structure);
        }

        if (CreatureLookup.ContainsKey(entity))
        {
            CreatureLookup.Remove(entity);
            CreatureIdLookup.Remove(entity.Id);
        }

        if (ItemLookup.ContainsKey(entity))
        {
            ItemLookup.Remove(entity);
            ItemIdLookup.Remove(entity.Id);
        }
    }
}