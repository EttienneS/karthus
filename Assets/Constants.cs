using UnityEngine;

public static class ColorConstants
{
    public static Color AbyssColor = Color.magenta;
    public static Color BaseColor = Color.white;
    public static Color BluePrintColor = new Color(0.3f, 1f, 1f, 0.4f);
    public static Color InvalidColor = Color.red;
    public static Color UnboundColor = new Color(0.66f, 0.18f, 0.6f, 0.9f);
}

public static class GameConstants
{
    public static float ChannelDuration = 2f;
}

public static class ControllerConstants
{
    public const string CameraController = "Main Camera";
    public const string CreatureController = "CreatureController";
    public const string GameController = "GameController";
    public const string ItemController = "ItemController";
    public const string MapController = "Map";
    public const string OrderSelectionController = "OrderPanel";
    public const string SpriteController = "SpriteStore";
    public const string StockpileController = "StockpileController";
    public const string StructureController = "StructureController";
    public const string TimeController = "TimePanel";
    public const string ManaDisplay = "ManaDisplay";
    public const string MaterialController = "MaterialController";
    public const string LeyLineController = "LeyLineController";
    public const string MagicController = "MagicController";
}

public static class LayerConstants
{
    public const string Border = "Border";
    public const string Bottom = "Default";
    public const string CarriedItem = "CarriedItem";
    public const string Creature = "Creature";
    public const string Fog = "Fog";
    public const string Item = "Item";
    public const string Stockpile = "Stockpile";
    public const string Structure = "Structure";
    public const string Tree = "Tree";
}