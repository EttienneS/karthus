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
            case ObjectType.Creature:
                return GetCreatureFromId(gameObjectId).LinkedGameObject.SpriteRenderer;

            case ObjectType.Item:
                return GetItemFromId(gameObjectId).LinkedGameObject.SpriteRenderer;

            case ObjectType.Structure:
                return GetStructureFromId(gameObjectId).LinkedGameObject.SpriteRenderer;

            case ObjectType.Stockpile:
                return GetStockpileFromId(gameObjectId).LinkedGameObject.SpriteRenderer;

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

    public static string GetGameId(this StockpileData stockpile)
    {
        return $"{StockpilePrefix}{stockpile.Id}";
    }

    public static string GetGameId(this ItemData item)
    {
        return $"{ItemPrefix}{item.Id}";
    }

    public static int GetId(string id)
    {
        return int.Parse(id.Split('-')[1]);
    }

    public static ItemData GetItemFromId(string id)
    {
        return Game.ItemController.ItemIdLookup[GetId(id)].Data;
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

        if (IsItem(id))
        {
            return GetItemFromId(id).LinkedGameObject.Cell.Coordinates;
        }

        if (IsStockpile(id))
        {
            return GetStockpileFromId(id).Coordinates;
        }

        return null;
    }

    public static StockpileData GetStockpileFromId(string id)
    {
        return Game.StockpileController.StockpileLookup[GetId(id)].Data;
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