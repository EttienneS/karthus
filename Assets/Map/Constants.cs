using UnityEngine;

public static class Constants
{
    // this is only one axis of the map so a size 5 map would be 5xCellPerBlock(25)  125 blocks tall and 125 cells wide (15625 cells total)
    internal const int MapSizeBlocks = 5;

    internal const int PixelsPerCell = 32;
    internal const int CellsPerTerrainBlock = 16;

    internal const float JitterProbability = 0.25f;

    internal const int MapSize = MapSizeBlocks * CellsPerTerrainBlock;

    internal const int PixelsPerBlock = CellsPerTerrainBlock * PixelsPerCell;

    internal static int TotalTextures = Mathf.CeilToInt(MapSize / CellsPerTerrainBlock);
}