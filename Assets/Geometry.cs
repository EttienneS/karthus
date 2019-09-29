using UnityEngine;

public static class Geometry
{
    // Explanation of PointInTriangle method:
    // youtu.be/HYAgJN3x4GA?list=PLFt_AvWsXl0cD2LPxcjxVjWTQLxJqKpgZ
    public static bool PointInTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        double s1 = C.y - A.y;
        double s2 = C.x - A.x;
        double s3 = B.y - A.y;
        double s4 = P.y - A.y;

        double w1 = (A.x * s1 + s4 * s2 - P.x * s1) / (s3 * s2 - (B.x - A.x) * s1);
        double w2 = (s4 - w1 * s3) / s1;
        return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
    }
}
