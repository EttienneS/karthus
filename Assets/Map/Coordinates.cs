using System;
using UnityEngine;

[Serializable]
public class Coordinates
{
    public int X;

    public int Y;

    public Coordinates(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static Coordinates FromPosition(Vector2 position)
    {
        // add half a unit to each position to account for offset (cells are at point 0,0 in the very center)
        position += new Vector2(0.5f, 0.5f);

        return new Coordinates(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
    }

    public int DistanceTo(Coordinates other)
    {
        return (X < other.X ? other.X - X : X - other.X)
                + (Y < other.Y ? other.Y - Y : Y - other.Y);
    }

    public override string ToString()
    {
        return "Cell: (" + X + ", " + Y + ")";
    }

    public string ToStringOnSeparateLines()
    {
        return X + "\n" + Y;
    }
}