using UnityEngine;

public class Coordinates
{
    public int X ;

    public int Y ;
    public Coordinates(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static Coordinates FromPosition(Vector3 position)
    {
        var x = position.x / Metrics.Width;
        var y = position.y / Metrics.Height;

        return new Coordinates((int)x, (int)y);
    }

    public override string ToString()
    {
        return "Cell: (" + X + ", " + Y + ")";
    }

    public string ToStringOnSeparateLines()
    {
        return X + "\n" + Y;
    }

    public int DistanceTo(Coordinates other)
    {
        return (X < other.X ? other.X - X : X - other.X)
                + (Y < other.Y ? other.Y - Y : Y - other.Y);
    }
}