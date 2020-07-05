using UnityEngine;

public static class ColorConstants
{
    public static Color BluePrintColor { get; } = new Color(0.3f, 1f, 1f, 0.3f);

    // palette: https://flatuicolors.com/palette/gb
    public static Color BlueBase { get; } = ColorExtensions.GetColorFromHex("#00a8ff");
    public static Color BlueAccent { get; } = ColorExtensions.GetColorFromHex("#0097e6");

    public static Color PurpleBase { get; } = ColorExtensions.GetColorFromHex("#9c88ff");
    public static Color PurpleAccent { get; } = ColorExtensions.GetColorFromHex("#8c7ae6");

    public static Color YellowBase { get; } = ColorExtensions.GetColorFromHex("#fbc531");
    public static Color YellowAccent { get; } = ColorExtensions.GetColorFromHex("#e1b12c");

    public static Color GreenBase { get; } = ColorExtensions.GetColorFromHex("#4cd137");
    public static Color GreenAccent { get; } = ColorExtensions.GetColorFromHex("#44bd32");

    public static Color GreyBlueBase { get; } = ColorExtensions.GetColorFromHex("#487eb0");
    public static Color GreyBlueAccent { get; } = ColorExtensions.GetColorFromHex("#40739e");

    public static Color RedBase { get; } = ColorExtensions.GetColorFromHex("#e84118");
    public static Color RedAccent { get; } = ColorExtensions.GetColorFromHex("#c23616");

    public static Color WhiteBase { get; } = ColorExtensions.GetColorFromHex("#f5f6fa");
    public static Color WhiteAccent { get; } = ColorExtensions.GetColorFromHex("#dcdde1");

    public static Color GreyBase { get; } = ColorExtensions.GetColorFromHex("#7f8fa6");
    public static Color GreyAccent { get; } = ColorExtensions.GetColorFromHex("#718093");

    public static Color DarkBlueBase { get; } = ColorExtensions.GetColorFromHex("#273c75");
    public static Color DarkBlueAccent { get; } = ColorExtensions.GetColorFromHex("#192a56");

    public static Color BlackBase { get; } = ColorExtensions.GetColorFromHex("#353b48");
    public static Color BlackAccent { get; } = ColorExtensions.GetColorFromHex("#2f3640");
}
