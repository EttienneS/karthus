using System;
using UnityEngine;

namespace Assets.Map
{
    public class MapGenerator : MonoBehaviour
    {
        public ChunkRenderer ChunkPrefab;

        public (string name, float maxTemp, float maxMoisture, Color color)[] Biomes;

        private float[,] _heightMap;

        private float[,] _moistureMap;

        private int _size;

        private float[,] _temperatureMap;

        public void Generate()
        {
            MapGenerationData.Instance = new MapGenerationData(Guid.NewGuid().ToString());

            var seed = MapGenerationData.Instance.Seed.GetHashCode();
            _size = MapGenerationData.Instance.MapSize * MapGenerationData.Instance.ChunkSize;
            _heightMap = GetHeightMap(seed);
            _temperatureMap = GetTemperatureMap(seed);
            _moistureMap = GetMoistureMap(seed);

            for (int x = 0; x < MapGenerationData.Instance.MapSize; x++)
            {
                for (int y = 0; y < MapGenerationData.Instance.MapSize; y++)
                {
                    MakeChunk(new Chunk(x, y, GetCells(x, y)));
                }
            }
        }

        public ChunkRenderer MakeChunk(Chunk data)
        {
            var chunk = Instantiate(ChunkPrefab, transform);
            chunk.transform.position = new Vector2(data.X * MapGenerationData.Instance.ChunkSize, data.Z * MapGenerationData.Instance.ChunkSize);
            chunk.name = $"Chunk: {data.X}_{data.Z}";
            chunk.Data = data;
            return chunk;
        }

        public float[,] GenerateRadialGradient(float normalize, float power)
        {
            float[,] grad = new float[_size, _size];

            float cX = _size * 0.5f;
            float cY = _size * 0.5f;

            for (int y = 0; y < _size; ++y)
            {
                for (int x = 0; x < _size; ++x)
                {
                    grad[x, y] = Mathf.Clamp(normalize - (float)(DistanceToCenter(cX, cY, x, y) / _size) * normalize * power, 0f, 2f);
                }
            }

            return grad;
        }

        private float DistanceToCenter(float cX, float cY, float x, float y)
        {
            return (float)Mathf.Sqrt(Mathf.Pow(x - cX, 2) + Mathf.Pow(y - cY, 2));
        }

        private float[,] GetHeightMap(int seed)
        {
            var noise = new FastNoiseLite(seed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
            noise.SetFrequency(0.04f);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);

            var height = noise.GetNoiseMap(_size);
            var radMap = GenerateRadialGradient(1f, 2f);
            for (int y = 0; y < _size; ++y)
            {
                for (int x = 0; x < _size; ++x)
                {
                    var value = height[x, y];
                    value = ((value + 1) / 2) * 9; // normalize to be between 0 and 9
                    value *= radMap[x, y]; // apply edge gradient

                    height[x, y] = value;
                }
            }

            return height;
        }

        private float[,] GetMoistureMap(int seed)
        {
            var noise = new FastNoiseLite(seed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFractalType(FastNoiseLite.FractalType.PingPong);

            var moistureMap = noise.GetNoiseMap(_size);

            for (int y = 0; y < _size; ++y)
            {
                for (int x = 0; x < _size; ++x)
                {
                    moistureMap[x, y] = (1 + moistureMap[x, y]) / 2f; // normalize moisture to be between 0 and 1
                }
            }

            return moistureMap;
        }

        private float[,] GetTemperatureMap(int seed)
        {
            var noise = new FastNoiseLite(seed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetFrequency(0.1f);

            var tempMap = noise.GetNoiseMap(_size);
            var heightPow = 0.1f;
            for (int y = 0; y < _size; ++y)
            {
                for (int x = 0; x < _size; ++x)
                {
                    var temp = tempMap[x, y];
                    temp = (1 + temp) / 2f; // normalize temperature to be between 0 and 1
                    temp -= _heightMap[x, y] * heightPow; // subtract height from temp (higher height == lower temp)

                    tempMap[x, y] = temp;
                }
            }

            return tempMap;
        }

        private void Start()
        {
            Biomes = new (string name, float maxTemp, float maxMoisture, Color color)[]{
                ("Tundra", 0.2f, 0.4f, Color.white),
                ("BorealForest", 0.4f, 0.4f, ColorExtensions.GetColorFromHex("2D6A4F")),
                ("Grassland", 0.6f, 0.4f, ColorExtensions.GetColorFromHex("52B788")),
                ("TemperateForest", 0.6f, 0.6f, ColorExtensions.GetColorFromHex("1B4332")),
                ("HighVeld", 0.8f, 0.5f, ColorExtensions.GetColorFromHex("8ea604")),
                ("Desert", 1f, 0.2f, ColorExtensions.GetColorFromHex("efd6ac")),
                ("Shrubland", 1f, 0.4f, ColorExtensions.GetColorFromHex("717744")),
                ("TropicalForest", 1f, 0.8f, ColorExtensions.GetColorFromHex("55a630"))
            };
            Generate();
        }

        private void Update()
        {
        }

        internal ChunkCell[,] GetCells(int offsetX, int offsetY)
        {
            var cells = new ChunkCell[MapGenerationData.Instance.ChunkSize, MapGenerationData.Instance.ChunkSize];
            offsetX *= MapGenerationData.Instance.ChunkSize;
            offsetY *= MapGenerationData.Instance.ChunkSize;
            for (var x = 0; x < MapGenerationData.Instance.ChunkSize; x++)
            {
                for (var y = 0; y < MapGenerationData.Instance.ChunkSize; y++)
                {
                    cells[x, y] = GetCellAt(offsetX + x, offsetY + y);
                }
            }

            return cells;
        }

        private Color GetBiome(float temp, float moisture)
        {
            foreach (var biome in Biomes)
            {
                if (temp > biome.maxTemp || moisture > biome.maxMoisture)
                {
                    continue;
                }
                return biome.color;
            }

            return Color.magenta;
        }

        private ChunkCell GetCellAt(int x, int y)
        {
            Debug.Log($"{x} {y}");

            var height = (int)_heightMap[x, y];

            if (height <= 0)
            {
                return new ChunkCell(-1f, Color.blue);
            }

            return new ChunkCell(height, GetBiome(_moistureMap[x, y], _temperatureMap[x, y]));
        }
    }
}