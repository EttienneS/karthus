using System;

[Flags]
public enum Direction
{
    N = 0, NE = 1, E = 2, SE = 3, S = 4, SW = 5, W = 6, NW = 7
}

public static class DirectionExtensions
{
    // 8    1    2
    // 7    C    3
    // 6    5    4
    // adding or subracting 4 will give the oposite cell
    public static Direction Opposite(this Direction direction)
    {
        return (int)direction < 4 ? direction + 4 : direction - 4;
    }

    public static Direction RotateCCW(this Direction rotation)
    {
        if (rotation == Direction.N)
        {
            rotation = Direction.NW;
        }
        else
        {
            rotation++;
        }
        return rotation;
    }

    public static Direction RotateCW(this Direction rotation)
    {
        if (rotation == Direction.NW)
        {
            rotation = Direction.N;
        }
        else
        {
            rotation--;
        }
        return rotation;
    }

    public static int ToDegrees(this Direction rotation)
    {
        return (int)rotation * 45;
    }
}