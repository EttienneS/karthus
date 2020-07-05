using System;

namespace Assets.Helpers
{
    public static class MapHelper
    {
        public static float GetDegreesToPoint(this Cell point1, Cell point2)
        {
            var deltaX = point1.X - point2.X;
            var deltaY = point1.Z - point2.Z;

            var radAngle = Math.Atan2(deltaY, deltaX);
            var degreeAngle = radAngle * 180.0 / Math.PI;

            return (float)(180.0 - degreeAngle);
        }
    }
}