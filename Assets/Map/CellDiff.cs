using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Map
{
    [Serializable]
    public class CellDiff
    {
        public CellDiff()
        {

        }

        public CellDiff(IEnumerable<Cell> cells, string biome, string region, float height) : this()
        {
            CellCollection = new ManagedCellCollection(cells);

            Biome = biome;
            Region = region;
            Height = height;
        }

        public string Biome { get; set; }
        public float Height { get; set; }
        public string Region { get; set; }

        public ManagedCellCollection CellCollection { get; set; }
    }
}