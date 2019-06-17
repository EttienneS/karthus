using UnityEngine;

public static class ColorConstants
{
    public static Color BaseColor = new Color(0.8f, 0.8f, 0.8f);
    public static Color BluePrintColor = new Color(0.3f, 1f, 1f, 0.4f);
    public static Color InvalidColor = Color.red;
    public static Color UnboundStructureColor = new Color(0.66f, 0.18f, 0.6f, 0.6f);
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
    public const string TimeController = "TimeManager";
    public const string ManaDisplay = "ManaDisplay";
    public const string MaterialController = "MaterialController";
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

public static class MapConstants
{
    internal const int CellsPerTerrainBlock = 12;

    internal const float JitterProbability = 0.25f;

    internal const int MapSize = MapSizeBlocks * CellsPerTerrainBlock;

    // this is only one axis of the map so a size 5 map would be 5xCellPerBlock(25)  125 blocks tall and 125 cells wide (15625 cells total)
    internal const int MapSizeBlocks = 5;

    internal const int PixelsPerBlock = CellsPerTerrainBlock * PixelsPerCell;
    internal const int PixelsPerCell = 32;
    internal static int TotalTextures = Mathf.CeilToInt(MapSize / CellsPerTerrainBlock);
}