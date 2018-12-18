using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Flags]
public enum Direction
{
    N, NE, E, SE, S, SW, W, NW
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
}
