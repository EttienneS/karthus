public static class GameIdHelper
{
    public const string CreaturePrefix = "C-";
    public const string ItemPrefix = "I-";
    public const string StockpilePrefix = "P-";
    public const string StructurePrefix = "S-";
    public static CreatureData GetCreatureFromId(string id)
    {
        return CreatureController.Instance.CreatureIdLookup[GetId(id)];
    }

    public static string GetGameId(this CreatureData creature)
    {
        return $"{CreaturePrefix}{creature.Id}";
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
        return ItemController.Instance.ItemIdLookup[GetId(id)].Data;
    }

    public static StockpileData GetStockpileFromId(string id)
    {
        return StockpileController.Instance.StockpileLookup[GetId(id)].Data;
    }

    public static StructureData GetStructureFromId(string id)
    {
        return StructureController.Instance.StructureIdLookup[GetId(id)];
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