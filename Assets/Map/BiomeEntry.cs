using Assets.Helpers;
using Camera;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Map
{


    [Serializable]
    public class BiomeEntry
    {
        public Color Color;

        [Range(0f, 1f)]
        public float MaxMoisture;

        [Range(0f, 1f)]
        public float MaxTemp;

        [Range(0f, 1f)]
        public float MinMoisture;

        [Range(0f, 1f)]
        public float MinTemp;

        public string Name;

        public BiomeEntry()
        {
        }

        public BiomeEntry(string name, float maxTemp, float maxMoisture, Color color) : this()
        {
            Name = name;
            MaxTemp = maxTemp;
            MaxMoisture = maxMoisture;
            Color = color;
        }
    }

}
