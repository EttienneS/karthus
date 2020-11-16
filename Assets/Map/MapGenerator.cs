using Assets.Helpers;
using Camera;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Map
{
    public class MapGenerator : MonoBehaviour
    {
        public BiomeEntry[] Biomes;
        public CameraController Camera;
        public Material ChunkMaterial;
        public ChunkRenderer ChunkPrefab;
        public AnimationCurve HeightCurve;
        public MapGenerationData MapGenerationData;
        public Pathfinder Pathfinder;
        public AnimationCurve TemperatureCurve;
        public AnimationCurve TerrainFallofCurve;
        private ChunkCell[,] _cells;

        private float[,] _heightMap;
        private int _missCounter;
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
                _temperatureMap = GetTemperatureMap();
                _moistureMap = GetMoistureMap(seed);

                _cells = GetCells();

                DrawChunks();

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

        public void MakeRivers()
        {
            var point = _cells[1, 1];

            var searcher = new JoinedAreaSearcher(point, (cell) => cell.Color == point.Color, int.MaxValue);
            var ocean = searcher.Resolve();
            foreach (var cell in ocean)
            {
                cell.Color = Color.black;
            }

            //var req1 = Pathfinder.Instance.CreatePathRequest(_cells[22, 5], _cells[41, 2], Mobility.Walk);
            //var req2 = Pathfinder.Instance.CreatePathRequest(_cells[5, 53], _cells[1, 12], Mobility.Walk);
            //var req3 = Pathfinder.Instance.CreatePathRequest(_cells[15, 5], _cells[11, 2], Mobility.Walk);
            //Pathfinder.Instance.ResolveAll();
            
            DestroyChunks();
            DrawChunks();
        }

        public void Regenerate()
        {
            DestroyChunks();
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
                    cells[x, y] = _cells[offsetX + x, offsetY + y];
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

        private void DestroyChunks()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        private float DistanceToCenter(float cX, float cY, float x, float y)
        {
            return (float)Mathf.Sqrt(Mathf.Pow(x - cX, 2) + Mathf.Pow(y - cY, 2));
        }

        private void DrawChunks()
        {
            for (int x = 0; x < MapGenerationData.Instance.MapSize; x++)
            {
                for (int y = 0; y < MapGenerationData.Instance.MapSize; y++)
                {
                    MakeChunk(new Chunk(x, y, GetCells(x, y)));
                }
            }
        }

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

        private ChunkCell[,] GetCells()
        {
            var cells = new ChunkCell[_size, _size];

            var waterCol = ColorExtensions.GetColorFromHex("7f7e7d");
            for (int x = 0; x < _size; x++)
            {
                for (int y = 0; y < _size; y++)
                {
                    var height = _heightMap[x, y];
                    cells[x, y] = new ChunkCell(x, y, height, height > 0 ? GetBiome(_temperatureMap[x, y], _moistureMap[x, y]) : waterCol);
                }
            }

            Pathfinder.LinkCellsToNeighbors(cells, _size);

            return cells;
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

                    if (value < 0)
                    {
                        value = -0.5f - (HeightCurve.Evaluate(-value) * 25f);
                    }
                    else
                    {
                        value = HeightCurve.Evaluate(value) * 10f;
                    }

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
                    // normalize moisture to be between 0 and 1
                    moistureMap[x, y] = Mathf.Clamp((1 + moistureMap[x, y]) / 2f, 0f, 1f);
                }
            }

            return moistureMap;
        }

        private float[,] GetTemperatureMap()
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
            // subtract height from temp (higher height == lower temp)
            // divide height by this to determine influence
            var heightPower = 10f;
            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    var temp = tempMap[x, y];
                    temp -= _heightMap[x, y] / heightPower;

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