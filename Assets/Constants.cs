using UnityEngine;

public static class ColorConstants
{
    public static Color AbyssColor = Color.magenta;
    public static Color BluePrintColor = new Color(0.3f, 1f, 1f, 0.4f);

    public static Color InvalidColor = Color.red;

    public static Color[] SkinArray = new[]
    {
        // https://www.schemecolor.com/skin-shades-color-scheme.php
        ColorExtensions.GetColorFromHex("#553827"),
        ColorExtensions.GetColorFromHex("#935934"),
        ColorExtensions.GetColorFromHex("#bd804a"),
        ColorExtensions.GetColorFromHex("#F4C7B1"),
        ColorExtensions.GetColorFromHex("#e6a17d"),
        ColorExtensions.GetColorFromHex("#be7f5e"),
    };

    public static Color[] HairArray = new[]
    {
        // https://www.schemecolor.com/chairing-the-meeting.php
        ColorExtensions.GetColorFromHex("#b8aca7"),
        ColorExtensions.GetColorFromHex("#E9E3DF"),
        ColorExtensions.GetColorFromHex("#DED4BB"),
        ColorExtensions.GetColorFromHex("#9a765f"),
        ColorExtensions.GetColorFromHex("#5d3721"),
        ColorExtensions.GetColorFromHex("#492a18"),
    };
}

public static class GameConstants
{
    public static float ChannelDuration = 0.5f;
}

public static class ControllerConstants
{
    public const string MinimapCamera = "MinimapCamera";
    public const string MainMenu = "MainMenu";
    public const string CameraController = "Main Camera";
    public const string PhysicsController = "PhysicsController";
    public const string CreatureController = "CreatureController";
    public const string GameController = "GameController";
    public const string ItemController = "ItemController";
    public const string MapController = "Map";
    public const string OrderSelectionController = "OrderPanel";
    public const string SpriteController = "SpriteStore";
    public const string StructureController = "StructureController";
    public const string TimeController = "TimeManager";
    public const string ManaDisplay = "ManaDisplay";
    public const string MaterialController = "MaterialController";
    public const string LeyLineController = "LeyLineController";
    public const string MagicController = "MagicController";
    public const string VisualEffectController = "VisualEffectController";
}

public static class LayerConstants
{
    public const string Border = "Border";
    public const string Bottom = "Default";
    public const string CarriedItem = "CarriedItem";
    public const string Creature = "Creature";
    public const string Fog = "Fog";
    public const string Item = "Item";
    public const string Zone = "Zone";
    public const string Structure = "Structure";
    public const string Tree = "Tree";
}