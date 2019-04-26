using System;
using UnityEngine;

public class Coordinates : IEquatable<Coordinates>
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

    public Vector2 ToMapVector()
    {
        // add half a unit to each position to account for offset (cells are at point 0,0 in the very center)
        return new Vector2(Mathf.FloorToInt(X) + 0.5f, Mathf.FloorToInt(Y) + 0.5f);
    }

    public Vector3 ToTopOfMapVector()
    {
        return new Vector3(Mathf.FloorToInt(X) + 0.5f, Mathf.FloorToInt(Y) + 0.5f, -1);
    }

    public static bool operator !=(Coordinates obj1, Coordinates obj2)
    {
        if (ReferenceEquals(obj1, null))
        {
            return !ReferenceEquals(obj2, null);
        }

        return !obj1.Equals(obj2);
    }

    public static bool operator ==(Coordinates obj1, Coordinates obj2)
    {
        if (ReferenceEquals(obj1, null))
        {
            return ReferenceEquals(obj2, null);
        }

        return obj1.Equals(obj2);
    }

    public int DistanceTo(Coordinates other)
    {
        return (X < other.X ? other.X - X : X - other.X)
                + (Y < other.Y ? other.Y - Y : Y - other.Y);
    }

    public bool Equals(Coordinates other)
    {
        if (ReferenceEquals(other, null))
        {
            return false;
        }

        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        var other = obj as Coordinates;
        if (other == null)
        {
            return false;
        }

        return this == other;
    }

    public override int GetHashCode()
    {
        return $"{X}:{Y}".GetHashCode();
    }

    public override string ToString()
    {
        return "Cell: (" + X + ", " + Y + ")";
    }

    public string ToStringOnSeparateLines()
    {
        return $"X: {X}\nY: {Y}";
    }
}