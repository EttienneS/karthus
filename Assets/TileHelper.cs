using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TileHelper
{
    public static void RotateTile(this Tile tile, Rotation rotation)
    {
        var m = tile.transform;
        var rot = Quaternion.Euler(0.0f, 0.0f, (int)rotation * 90);
        m.SetTRS(Vector3.zero, rot, Vector3.one);
        tile.transform = m;
    }

}
