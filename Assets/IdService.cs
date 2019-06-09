using System;
using UnityEngine;

public static class IdService
{
    public const string CreaturePrefix = "C-";
    public const string ItemPrefix = "I-";
    public const string StockpilePrefix = "P-";
    public const string StructurePrefix = "S-";

    public static CreatureData GetCreatureFromId(string id)
    {
        return Game.CreatureController.CreatureIdLookup[GetId(id)];
    }

    public static string GetGameId(this CreatureData creature)
    {
        return $"{CreaturePrefix}{creature.Id}";
    }

    internal static SpriteRenderer GetSpriteRendererForId(string gameObjectId)
    {
        switch (GetObjectTypeForId(gameObjectId))
        {
            //case ObjectType.Creature:
            //    return GetCreatureFromId(gameObjectId).LinkedGameObject.SpriteRenderer;

            case ObjectType.Structure:
                return GetStructureFromId(gameObjectId).LinkedGameObject.SpriteRenderer;

            default:
                throw new NotImplementedException();
        }
    }

    public enum ObjectType
    {
        Creature, Item, Structure, Stockpile
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
        if (IsItem(gameObjectId))
        {
            return ObjectType.Item;
        }
        if (IsStockpile(gameObjectId))
        {
            return ObjectType.Stockpile;
        }

        throw new NotImplementedException();
    }

    public static string GetGameId(this StructureData structure)
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


    public static StructureData GetStructureFromId(string id)
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
}