using System;
using System.Collections.Generic;
using UnityEngine;

public static class IdService
{
    public static Dictionary<IEntity, CreatureData> CreatureLookup = new Dictionary<IEntity, CreatureData>();
    public static Dictionary<IEntity, Structure> StructureLookup = new Dictionary<IEntity, Structure>();

    public static Dictionary<string, CreatureData> CreatureIdLookup = new Dictionary<string, CreatureData>();
    public static Dictionary<string, Structure> StructureIdLookup = new Dictionary<string, Structure>();

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
        else if (entity is CreatureData creature)
        {
            CreatureLookup.Add(entity, creature);
            CreatureIdLookup.Add(entity.Id, creature);
        }
        else
        {
            throw new NotImplementedException("Unknown entity type!");
        }
    }

    private static int _idCounter = 0;

    public enum ObjectType
    {
        Creature, Structure
    }

    public static CreatureData GetCreatureFromId(string id)
    {
        return IdService.CreatureIdLookup[id];
    }

    internal static Faction GetFactionForId(string id)
    {
        if (IsCreature(id))
        {
            return GetCreatureFromId(id).Faction;
        }
        if (IsStructure(id))
        {
            return GetStructureFromId(id).Faction;
        }

        return null;
    }

    public static Coordinates GetLocation(string id)
    {
        if (IsCreature(id))
        {
            return GetCreatureFromId(id).Coordinates;
        }
        if (IsStructure(id))
        {
            return GetStructureFromId(id).Coordinates;
        }

        return null;
    }

    public static Structure GetStructureFromId(string id)
    {
        return StructureIdLookup[id];
    }

    public static bool IsCreature(string id)
    {
        return CreatureIdLookup.ContainsKey(id);
    }

    public static bool IsStructure(string id)
    {
        return StructureIdLookup.ContainsKey(id);
    }

    internal static IMagicAttuned GetMagicAttuned(string gameObjectId)
    {
        switch (GetObjectTypeForId(gameObjectId))
        {
            case ObjectType.Creature:
                return GetCreatureFromId(gameObjectId);

            case ObjectType.Structure:
                return GetStructureFromId(gameObjectId);

            default:
                Debug.Log($"{gameObjectId} is not magic attuned");
                return null;
        }
    }

    internal static void Clear()
    {
        StructureLookup.Clear();
        StructureIdLookup.Clear();
        CreatureLookup.Clear();
        CreatureIdLookup.Clear();
    }

    internal static ObjectType GetObjectTypeForId(string gameObjectId)
    {
        if (IsStructure(gameObjectId))
        {
            return ObjectType.Structure;
        }
        if (IsCreature(gameObjectId))
        {
            return ObjectType.Creature;
        }

        throw new NotImplementedException();
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