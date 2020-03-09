using System;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class IdService
{
    public Dictionary<string, Creature> CreatureIdLookup = new Dictionary<string, Creature>();
    public Dictionary<IEntity, Creature> CreatureLookup = new Dictionary<IEntity, Creature>();
    public Dictionary<string, Item> ItemIdLookup = new Dictionary<string, Item>();
    public Dictionary<IEntity, Item> ItemLookup = new Dictionary<IEntity, Item>();
    public Dictionary<string, Structure> StructureIdLookup = new Dictionary<string, Structure>();
    public Dictionary<IEntity, Structure> StructureLookup = new Dictionary<IEntity, Structure>();
    public Dictionary<Cell, List<Structure>> StructureCellLookup = new Dictionary<Cell, List<Structure>>();
    private int _idCounter;

    public void EnrollEntity(IEntity entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            entity.Id = _idCounter.ToString();
            _idCounter++;
        }
        else
        {
            var id = int.Parse(entity.Id);
            if (id > _idCounter)
            {
                _idCounter = id + 1;
            }
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
        StructureLookup.Clear();
        StructureIdLookup.Clear();

        CreatureLookup.Clear();
        CreatureIdLookup.Clear();

        ItemLookup.Clear();
        ItemIdLookup.Clear();
    }

    internal void DestroyEntity(IEntity entity)
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

    internal void RemoveEntity(IEntity entity)
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

public static class IdExtensions
{
    public static Creature GetCreature(this string id)
    {
        if (!Game.IdService.CreatureIdLookup.TryGetValue(id, out var creature))
        {
            return null;
        }
        return creature;
    }

    public static Item GetItem(this string id)
    {
        if (!Game.IdService.ItemIdLookup.TryGetValue(id, out var item))
        {
            return null;
        }
        return item;
    }

    public static Structure GetStructure(this string id)
    {
        if (Game.IdService.StructureIdLookup.TryGetValue(id, out var structure))
        {
            return structure;
        }
        return null;
    }

    public static Container GetContainer(this string id)
    {
        return GetStructure(id) as Container;
    }

    internal static IEntity GetEntity(this string id)
    {
        if (Game.IdService.IsCreature(id))
        {
            return GetCreature(id);
        }
        if (Game.IdService.IsStructure(id))
        {
            return GetStructure(id);
        }
        if (Game.IdService.IsItem(id))
        {
            return GetItem(id);
        }

        Debug.LogWarning("Unknown entity type!");
        return null;
    }
}