public enum Rotation
{
    N = 0, E = 1, S = 2, W = 3
}

public static class RotationHelper
{
    public static Rotation RotateCW(this Rotation rotation)
    {
        if (rotation == Rotation.W)
        {
            rotation = Rotation.N;
        }
        else
        {
            rotation++;
        }
        return rotation;
    }

    public static Rotation RotateCCW(this Rotation rotation)
    {
        if (rotation == Rotation.N)
        {
            rotation = Rotation.W;
        }
        else
        {
            rotation--;
        }
        return rotation;
    }
}
