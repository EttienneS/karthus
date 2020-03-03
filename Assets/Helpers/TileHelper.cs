using UnityEngine;
using UnityEngine.Tilemaps;

public static class TileHelper
{
    private static Direction[] _random90RotationOptions = new[] { Direction.N, Direction.E, Direction.S, Direction.W };

    public static void RotateRandom90(this Tile tile)
    {
        var m = tile.transform;

        var rot = Quaternion.Euler(0.0f, 0.0f, _random90RotationOptions.GetRandomItem().ToDegrees());
        m.SetTRS(Vector3.zero, rot, Vector3.one);
        tile.transform = m;
    }

    public static void RotateTile(this Tile tile, Direction rotation)
    {
        var m = tile.transform;
        var rot = Quaternion.Euler(0.0f, 0.0f, rotation.ToDegrees());
        m.SetTRS(Vector3.zero, rot, Vector3.one);
        tile.transform = m;
    }

    public static void ShiftTile(this Tile tile, Vector2 offset)
    {
        var m = tile.transform;
        m.SetTRS(offset, m.rotation, Vector3.one);
        tile.transform = m;
    }
}