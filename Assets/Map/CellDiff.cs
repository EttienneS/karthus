using System;

namespace Assets.Map
{
    [Serializable]
    public class CellDiff
    {
        public CellDiff(int x, int z, string regionName, int region, float height)
        {
            X = x;
            Z = z;
            BiomeName = regionName;
            BiomeRegion = region;
            Height = height;
        }

        public string BiomeName { get; set; }
        public int BiomeRegion { get; set; }
        public float Height { get; set; }
        public int X { get; set; }
        public int Z { get; set; }
    }
}