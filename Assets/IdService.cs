using System;
using UnityEngine;

public static class IdService
{
    public const string CreaturePrefix = "C-";
    public const string ItemPrefix = "I-";
    public const string StockpilePrefix = "P-";
    public const string StructurePrefix = "S-";

    private static int _idCounter = 0;

    public enum ObjectType
    {
        Creature, Structure
    }

    public static CreatureData GetCreatureFromId(string id)
    {
        return Game.CreatureController.CreatureIdLookup[GetId(id)];
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

    public static string GetGameId(this CreatureData creature)
    {
        return $"{CreaturePrefix}{creature.Id}";
    }

    public static string GetGameId(this Structure structure)
    {
        return $"{StructurePrefix}{structure.Id}";
    }

    public static int GetId(string id)
    {
        return int.Parse(id.Split('-')[1]);
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
        return Game.StructureController.StructureIdLookup[GetId(id)];
    }

    public static bool IsCreature(string id)
    {
        return id.StartsWith(CreaturePrefix);
    }

    public static bool IsItem(string id)
    {
        return id.StartsWith(ItemPrefix);
    }

    public static bool IsStockpile(string id)
    {
        return id.StartsWith(StockpilePrefix);
    }

    public static bool IsStructure(string id)
    {
        return id.StartsWith(StructurePrefix);
    }

    public static int UniqueId()
    {
        var id = _idCounter;
        _idCounter++;
        return id;
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
}