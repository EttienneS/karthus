using Assets.Helpers;
using Camera;
using System;
using UnityEngine;

namespace Assets.Map
{
    public class MapGenerator : MonoBehaviour
    {
        public BiomeEntry[] Biomes;
        public Material ChunkMaterial;
        public ChunkRenderer ChunkPrefab;
        public AnimationCurve TerrainFallofCurve;
        public AnimationCurve TemperatureCurve;
        public AnimationCurve HeightCurve;
        public MapGenerationData MapGenerationData;
        public CameraController Camera;

        private float[,] _heightMap;
        private float[,] _moistureMap;
        private int _size;
        private float[,] _temperatureMap;


        public void Generate()
        {
            using (Instrumenter.Start())
            {
                _missCounter = 0;

               MapGenerationData.Instance = MapGenerationData;

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

                Debug.Log($"Miss: {_missCounter}/{_size * _size}");

                Camera.ConfigureMinMax(0, _size, 0, (int)(_size * 1.5f));
                Camera.MoveToWorldCenter();
            }
        }

        public float[,] GenerateTerrainCircularFalloffMap()
        {
            float[,] grad = new float[_size, _size];

            float cX = _size * 0.5f;
            float cY = _size * 0.5f;

            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    grad[x, y] = TerrainFallofCurve.Evaluate((float)(DistanceToCenter(cX, cY, x, y) / (_size * 0.55f)));
                }
            }

            return grad;
        }

        public ChunkRenderer MakeChunk(Chunk data)
        {
            var chunk = Instantiate(ChunkPrefab, transform);
            chunk.transform.position = new Vector2(data.X * MapGenerationData.Instance.ChunkSize, data.Z * MapGenerationData.Instance.ChunkSize);
            chunk.name = $"Chunk: {data.X}_{data.Z}";
            chunk.Data = data;
            chunk.MeshRenderer.SetAllMaterial(ChunkMaterial);
            return chunk;
        }

        public void Regenerate()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            MapGenerationData.Seed = Guid.NewGuid().ToString();
            Generate();
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

        private static float CalculateTemperature(float equator, int y)
        {
            // calculate the absolute distance to the equator,
            // then get the proportion of the distance (normalize value to be between 0 and 1)
            // finally subtract that from 1 to get the inverse value (temp goes down as we move away from the equator)
            return 1f - (Math.Abs(y - equator) / equator);
        }

        private float DistanceToCenter(float cX, float cY, float x, float y)
        {
            return (float)Mathf.Sqrt(Mathf.Pow(x - cX, 2) + Mathf.Pow(y - cY, 2));
        }

        private int _missCounter;

        private Color GetBiome(float temp, float moisture)
        {
            foreach (var biome in Biomes)
            {
                if (temp >= biome.MinTemp && temp <= biome.MaxTemp &&
                   moisture >= biome.MinMoisture && moisture <= biome.MaxMoisture)
                {
                    return biome.Color;
                }
            }

            _missCounter++;
            Debug.Log($"No value for '{temp}t' '{moisture}m'");
            return Color.magenta;
        }

        private ChunkCell GetCellAt(int x, int y)
        {
            var height = _heightMap[x, y];

            if (height < 0)
            {
                return new ChunkCell(-0.5f -(HeightCurve.Evaluate(-height) * 25f), ColorExtensions.GetColorFromHex("7f7e7d"));
            }

            return new ChunkCell(HeightCurve.Evaluate(height) * 10f, GetBiome(_temperatureMap[x, y], _moistureMap[x, y]));
        }

        private float[,] GetHeightMap(int seed)
        {
            var noise = new FastNoiseLite(seed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(0.01f);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);

            var height = noise.GetNoiseMap(_size);
            var radMap = GenerateTerrainCircularFalloffMap();
            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    var value = height[x, y];
                    var rad = radMap[x, y];

                    value -= rad; // apply edge gradient

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

            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    var value = Mathf.Clamp((1 + moistureMap[x, y]) / 2f, 0f, 1f); // normalize moisture to be between 0 and 1
                    moistureMap[x, y] = value;
                }
            }

            return moistureMap;
        }

        private float[,] GetTemperatureMap(int seed)
        {
            var tempMap = new float[_size, _size];
            var equator = _size / 2f;
            for (int y = 0; y < _size; y++)
            {
                var temperature = CalculateTemperature(equator, y);

                for (int x = 0; x < _size; x++)
                {
                    tempMap[x, y] = TemperatureCurve.Evaluate(temperature);
                }
            }

            // apply height map to temp map
            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    var temp = tempMap[x, y];
                    temp -= (HeightCurve.Evaluate(_heightMap[x, y])); // subtract height from temp (higher height == lower temp)

                    tempMap[x, y] = Mathf.Clamp(temp, 0f, 1f);
                }
            }

            return tempMap;
        }

        private void Start()
        {
            Generate();
        }

        private void Update()
        {
        }
    }
}

[Serializable]
public class BiomeEntry
{
    public string Name;

    [Range(0f, 1f)]
    public float MinTemp;

    [Range(0f, 1f)]
    public float MaxTemp;

    [Range(0f, 1f)]
    public float MinMoisture;

    [Range(0f, 1f)]
    public float MaxMoisture;

    public Color Color;

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