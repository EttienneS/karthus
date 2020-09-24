using System;
using System.Collections.Generic;

namespace Assets.Map
{
    [Serializable]
    public class MapGenerationData
    {
        public int ChunkSize = 60;
        public bool CreateWater = true;
        public int CreaturesToSpawn = 3;
        public List<CellDiff> MapDiff = new List<CellDiff>();
        public bool Populate = true;
        public string Seed;
        public int Size = 1;
        public float WaterLevel = 0.5f;

        public MapGenerationData()
        {
        }

        public MapGenerationData(string seed)
        {
            Seed = seed;
        }

        public static MapGenerationData Instance { get; set; }

        internal void AddChange(IEnumerable<Cell> cells, string biome, string region, float height)
        {
            MapDiff.Add(new CellDiff(cells, biome, region, height));
        }
    }
}