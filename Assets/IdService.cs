using System;
using System.Collections.Generic;
using UnityEngine;

public static class IdService
{
    public static Dictionary<string, Creature> CreatureIdLookup = new Dictionary<string, Creature>();
    public static Dictionary<IEntity, Creature> CreatureLookup = new Dictionary<IEntity, Creature>();
    public static Dictionary<string, Structure> StructureIdLookup = new Dictionary<string, Structure>();
    public static Dictionary<IEntity, Structure> StructureLookup = new Dictionary<IEntity, Structure>();
    private static int _idCounter = 0;

    public enum ObjectType
    {
        Creature, Structure
    }

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
        }
        else if (entity is Creature creature)
        {
            CreatureLookup.Add(entity, creature);
            CreatureIdLookup.Add(entity.Id, creature);
        }
        else
        {
            throw new NotImplementedException("Unknown entity type!");
        }
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
    }

    public static Creature GetCreature(this string id)
    {
        if (!CreatureIdLookup.TryGetValue(id, out var creature))
        {
            return null;
        }
        return creature;
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

        Debug.LogWarning("Unknown entity type!");
        return null;
    }

    internal static void RemoveEntity(IEntity entity)
    {
        if (StructureLookup.ContainsKey(entity))
        {
            StructureLookup.Remove(entity);
            StructureIdLookup.Remove(entity.Id);
        }

        if (CreatureLookup.ContainsKey(entity))
        {
            CreatureLookup.Remove(entity);
            CreatureIdLookup.Remove(entity.Id);
        }
    }
}