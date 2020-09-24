using Assets.Helpers;
using Assets.ServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Assets.Map
{
    public class MapController : MonoBehaviour, IGameService
    {
        public const int PixelsPerCell = 64;
        public Dictionary<(int x, int z), Cell> CellLookup = new Dictionary<(int x, int z), Cell>();
        public List<Cell> Cells = new List<Cell>();
        public ChunkRenderer ChunkPrefab;

        public Light GlobalLight;
        public NoiseSettings LocalNoise;
        public GameObject WaterPrefab;
        public NoiseSettings WorldNoise;

        internal Dictionary<(int x, int y), ChunkRenderer> Chunks;
        private Dictionary<string, Biome> _biomeTemplates;
        private float[,] _localNoiseMap;
        private int? _seedValue;

        public Dictionary<string, Biome> BiomeTemplates
        {
            get
            {
                if (_biomeTemplates == null)
                {
                    _biomeTemplates = new Dictionary<string, Biome>();
                    foreach (var biomeFile in Loc.GetFileController().BiomeFiles)
                    {
                        _biomeTemplates.Add(biomeFile.name, biomeFile.text.LoadJson<Biome>());
                    }
                }

                return _biomeTemplates;
            }
        }

        public Cell Center
        {
            get
            {
                var x = MapGenerationData.Instance.Size * MapGenerationData.Instance.ChunkSize / 2;
                var z = MapGenerationData.Instance.Size * MapGenerationData.Instance.ChunkSize / 2;
                return CellLookup[(x, z)];
            }
        }

        public float[,] LocalNoiseMap
        {
            get
            {
                if (_localNoiseMap == null)
                {
                    _localNoiseMap = Noise.GenerateNoiseMap(SeedValue, MaxSize, MaxSize, LocalNoise, Vector2.zero);
                }
                return _localNoiseMap;
            }
        }

        internal int MaxX => MaxSize;
        internal int MaxZ => MaxSize;
        internal int MinX => 0;
        internal int MinZ => 0;

        internal int SeedValue
        {
            get
            {
                if (!_seedValue.HasValue)
                {
                    var md5Hasher = MD5.Create();
                    var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(MapGenerationData.Instance.Seed));
                    _seedValue = BitConverter.ToInt32(hashed, 0);
                }
                return _seedValue.Value;
            }
        }

        public void AddCellIfValid(int x, int z, List<Cell> cells)
        {
            if (CellLookup.ContainsKey((x, z)))
            {
                cells.Add(CellLookup[(x, z)]);
            }
        }

        public Cell CreateCell(int x, int z)
        {
            var noiseMapHeight = Loc.GetMap().GetNoiseMapPoint(x, z);
            var biome = Loc.GetMap().GetBiome(x, z).GetRegion(noiseMapHeight);

            var y = 0;
            var cell = new Cell
            {
                X = x,
                BiomeRegion = biome,
                Y = y,
                Z = z,
                SearchPhase = 0
            };

            Loc.GetMap().CellLookup.Add((x, z), cell);

            return cell;
        }

        public void GenerateMap()
        {
            var size = MaxSize;
            MakeCells(size);
            Loc.GetMap().Chunks = new Dictionary<(int x, int y), ChunkRenderer>();

            if (SaveManager.SaveToLoad == null)
            {
                for (var x = 0; x < MapGenerationData.Instance.Size; x++)
                {
                    for (var y = 0; y < MapGenerationData.Instance.Size; y++)
                    {
                        Loc.GetMap().MakeChunk(new Chunk(x, y));
                    }
                }
                PopulateCells();
            }
            else
            {
                foreach (var chunk in SaveManager.SaveToLoad.Chunks)
                {
                    Loc.GetMap().MakeChunk(chunk);
                }
                PopulateMapFromSave();
            }
        }

        private void PopulateMapFromSave()
        {
            Loc.GetTimeManager().Data = SaveManager.SaveToLoad.Time;
            foreach (var item in SaveManager.SaveToLoad.Items)
            {
                Loc.GetItemController().SpawnItem(item);
            }

            foreach (var faction in SaveManager.SaveToLoad.Factions)
            {
                Loc.GetFactionController().Factions.Add(faction.FactionName, faction);

                foreach (var creature in faction.Creatures.ToList())
                {
                    Loc.GetCreatureController().SpawnCreature(creature, creature.Cell, faction);
                }

                foreach (var structure in faction.Structures.ToList())
                {
                    Loc.GetStructureController().SpawnStructure(structure);
                }

                foreach (var blueprint in faction.Blueprints.ToList())
                {
                    Loc.GetStructureController().SpawnBlueprint(blueprint);
                }
            }

            SaveManager.SaveToLoad.Stores.ForEach(Loc.GetZoneController().LoadStore);
            SaveManager.SaveToLoad.Rooms.ForEach(Loc.GetZoneController().LoadRoom);
            SaveManager.SaveToLoad.Areas.ForEach(Loc.GetZoneController().LoadArea);

            //SaveManager.SaveToLoad.CameraData.Load(CameraController);
            SaveManager.SaveToLoad = null;
        }

        public Biome GetBiome(int x, int y)
        {
            //return BiomeTemplates["Debug"];
            return BiomeTemplates["Default"];
        }

        public Cell GetCellAtCoordinate(float x, float z)
        {
            var intx = Mathf.RoundToInt(x - 0.001f);
            var intz = Mathf.RoundToInt(z - 0.001f);
            if (intx < MinX || intz < MinZ || intx >= MaxX || intz >= MaxZ)
            {
                return null;
            }

            return CellLookup[(intx, intz)];
        }

        public Cell GetCellAtCoordinate(Vector3 pos)
        {
            return GetCellAtCoordinate(pos.x, pos.z);
        }

        public Cell GetCellAtPoint(Vector3 position)
        {
            // subtract half a unit to compensate for cell offset
            var cell = Cell.FromPosition(position - new Vector3(0.5f, 0.5f));

            if (cell == null ||
                cell.X < MinX || cell.Z < MinZ ||
                cell.X >= MaxX || cell.Z >= MaxZ)
            {
                return null;
            }
            return CellLookup[(cell.X, cell.Z)];
        }

        internal Vector3 GetMapCenter()
        {
            return new Vector3((MapGenerationData.Instance.ChunkSize * MapGenerationData.Instance.Size) / 2, 1, (MapGenerationData.Instance.ChunkSize * MapGenerationData.Instance.Size) / 2);
        }

        public Cell GetCellAttRadian(Cell center, int radius, int angle)
        {
            var mineX = Mathf.Clamp(Mathf.FloorToInt(center.X + (radius * Mathf.Cos(angle))), 0, MaxX);
            var mineY = Mathf.Clamp(Mathf.FloorToInt(center.Z + (radius * Mathf.Sin(angle))), 0, MaxZ);

            return GetCellAtCoordinate(mineX, mineY);
        }

        public List<Cell> GetCircle(Cell center, int radius)
        {
            var cells = new List<Cell>();

            var centerX = center.X;
            var centerY = center.Z;

            for (var x = centerX - radius; x <= centerX; x++)
            {
                for (var y = centerY - radius; y <= centerY; y++)
                {
                    if ((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY) <= radius * radius)
                    {
                        // calculate and add cell for each of the four points
                        AddCellIfValid(centerX - (x - centerX), centerY - (y - centerY), cells);
                        AddCellIfValid(centerX + (x - centerX), centerY + (y - centerY), cells);
                        AddCellIfValid(centerX - (x - centerX), centerY + (y - centerY), cells);
                        AddCellIfValid(centerX + (x - centerX), centerY - (y - centerY), cells);
                    }
                }
            }

            return cells;
        }

        public (Cell bottomLeft, Cell bottomRight, Cell topLeft, Cell topRight) GetCorners(List<Cell> square)
        {
            var (minx, maxx, minz, maxz) = Loc.GetMap().GetMinMax(square);

            return (Loc.GetMap().GetCellAtCoordinate(minx, minz),
                    Loc.GetMap().GetCellAtCoordinate(maxx, minz),
                    Loc.GetMap().GetCellAtCoordinate(minx, maxz),
                    Loc.GetMap().GetCellAtCoordinate(maxx, maxz));
        }

        public (int minx, int maxx, int minz, int maxz) GetMinMax(List<Cell> cells)
        {
            var minx = int.MaxValue;
            var maxx = int.MinValue;

            var minz = int.MaxValue;
            var maxz = int.MinValue;

            foreach (var cell in cells)
            {
                if (cell.X > maxx)
                {
                    maxx = cell.X;
                }
                if (cell.X < minx)
                {
                    minx = cell.X;
                }
                if (cell.Z > maxz)
                {
                    maxz = cell.Z;
                }
                if (cell.Z < minz)
                {
                    minz = cell.Z;
                }
            }

            return (minx, maxx, minz, maxz);
        }

        public Cell GetNearestEmptyCell(Cell cell)
        {
            var circle = GetCircle(cell, 1);

            for (int i = 2; i < 15; i++)
            {
                var newCircle = GetCircle(cell, i);
                newCircle.RemoveAll(c => circle.Contains(c));
                circle = newCircle;

                var empty = circle.Where(c => c.Structures.Count == 0);

                if (empty.Count() > 0)
                {
                    return empty.GetRandomItem();
                }
            }

            return null;
        }

        public float GetNoiseMapPoint(float x, float y)
        {
            return LocalNoiseMap[(int)x, (int)y];
        }

        public ChunkRenderer MakeChunk(Chunk data)
        {
            var chunk = Instantiate(ChunkPrefab, transform);
            chunk.transform.position = new Vector2(data.X * MapGenerationData.Instance.ChunkSize, data.Z * MapGenerationData.Instance.ChunkSize);
            chunk.name = $"Chunk: {data.X}_{data.Z}";
            chunk.Data = data;

            Loc.GetMap().Chunks.Add((data.X, data.Z), chunk);

            return chunk;
        }

        public void MakeFactionBootStrap(Faction faction)
        {
            var center = Loc.GetMap().GetNearestPathableCell(Loc.GetMap().Center, Mobility.Walk, 10);

            Loc.GetFactionController()
                         .PlayerFaction
                         .DomainCells
                         .AddCells(Loc.GetMap().GetCircle(Loc.GetMap().Center, 15));

            var open = Loc.GetMap().GetCircle(center, 5).Where(c => c.PathableWith(Mobility.Walk));
            Loc.GetItemController().SpawnItem("Berries", open.GetRandomItem(), 50);
            Loc.GetItemController().SpawnItem("Berries", open.GetRandomItem(), 50);
            Loc.GetItemController().SpawnItem("Plank", open.GetRandomItem(), 50);
            Loc.GetItemController().SpawnItem("Plank", open.GetRandomItem(), 50);
            Loc.GetItemController().SpawnItem("Stone", open.GetRandomItem(), 50);
            Loc.GetItemController().SpawnItem("Stone", open.GetRandomItem(), 50);
            Loc.GetItemController().SpawnItem("Stone", open.GetRandomItem(), 50);

            Loc.GetStructureController().SpawnStructure("Campfire", open.GetRandomItem(), Loc.GetFactionController().PlayerFaction);

            for (int i = 0; i < MapGenerationData.Instance.CreaturesToSpawn; i++)
            {
                var c = Loc.GetCreatureController().SpawnCreature(Loc.GetCreatureController().GetCreatureOfType("Person"),
                                                                       Loc.GetMap().GetNearestPathableCell(center, Mobility.Walk, 10),
                                                                       faction);
            }
        }

        public void SpawnCreatures()
        {
            foreach (var monster in Loc.GetCreatureController().Beastiary)
            {
                if (monster.Key == "Person")
                {
                    continue;
                }

                //for (int i = 0; i < Loc.GetGameController().MapData.CreaturesToSpawn; i++)
                //{
                //    var creature = Loc.GetCreatureController().GetCreatureOfType(monster.Key);

                //    var spot = Loc.GetMap().GetCircle(Instance.Center, 25).GetRandomItem();
                //    if (spot.TravelCost <= 0 && creature.Mobility != Mobility.Fly)
                //    {
                //        spot = Loc.GetMap().CellLookup.Values.Where(c => c.TravelCost > 0).GetRandomItem();
                //    }
                //    Loc.GetCreatureController().SpawnCreature(creature, spot, Loc.GetFactionController().MonsterFaction);
                //}
            }
        }

        internal Cell GetNearestPathableCell(Cell centerPoint, Mobility mobility, int radius)
        {
            var circle = GetCircle(centerPoint, radius);

            return circle.Where(c => c != centerPoint && c.PathableWith(mobility))
                               .OrderBy(c => c.DistanceTo(centerPoint))
                               .First();
        }

        internal void MakeCells(int size)
        {
            using (Instrumenter.Start())
            {
                InstantiateCells(size);
                LinkCellsToNeighbors(size);
            }
        }

        private static void LinkCellsToNeighbors(int size)
        {
            // link cell to the others in the chunk
            for (var z = 0; z < 0 + size; z++)
            {
                for (var x = 0; x < 0 + size; x++)
                {
                    var cell = Loc.GetMap().CellLookup[(x, z)];
                    if (x > 0)
                    {
                        cell.SetNeighbor(Direction.W, Loc.GetMap().CellLookup[(x - 1, z)]);

                        if (z > 0)
                        {
                            cell.SetNeighbor(Direction.SW, Loc.GetMap().CellLookup[(x - 1, z - 1)]);

                            if (x < size - 1)
                            {
                                cell.SetNeighbor(Direction.SE, Loc.GetMap().CellLookup[(x + 1, z - 1)]);
                            }
                        }
                    }

                    if (z > 0)
                    {
                        cell.SetNeighbor(Direction.S, Loc.GetMap().CellLookup[(x, z - 1)]);
                    }
                }
            }
        }

        private void InstantiateCells(int size)
        {
            Loc.GetMap().Cells = new List<Cell>();
            for (var y = 0; y < 0 + size; y++)
            {
                for (var x = 0; x < 0 + size; x++)
                {
                    Loc.GetMap().Cells.Add(CreateCell(x, y));
                }
            }
        }

        internal Cell TryGetPathableNeighbour(Cell coordinates)
        {
            var pathables = coordinates.NonNullNeighbors
                         .Where(c => c.TravelCost > 0)
                         .ToList();

            if (pathables.Count > 0)
            {
                return pathables.GetRandomItem();
            }
            return null;
        }

        private void PopulateCells()
        {
            using (Instrumenter.Start())
            {
                if (MapGenerationData.Instance.Populate)
                {
                    foreach (var cell in Loc.GetMap().Cells)
                    {
                        Populate(cell);
                    }

                    MakeFactionBootStrap(Loc.GetFactionController().PlayerFaction);
                    SpawnCreatures();
                }
            }
        }

        internal void Populate(Cell cell)
        {
            if (!cell.Empty())
            {
                return;
            }

            var content = cell.BiomeRegion.GetContent();

            if (!string.IsNullOrEmpty(content))
            {
                if (Loc.GetStructureController().StructureTypeFileMap.ContainsKey(content))
                {
                    Loc.GetStructureController().SpawnStructure(content, cell, Loc.GetFactionController().Factions[FactionConstants.World]);
                }
                else
                {
                    Loc.GetItemController().SpawnItem(content, cell);
                }
            }
        }

        public int MaxSize => MapGenerationData.Instance.Size * MapGenerationData.Instance.ChunkSize;

        public void Initialize()
        {
            if (SaveManager.SaveToLoad != null)
            {
                MapGenerationData.Instance = SaveManager.SaveToLoad.MapGenerationData;
            }
            else
            {
                if (MapGenerationData.Instance == null)
                {
                    MapGenerationData.Instance = new MapGenerationData(NameHelper.GetRandomName() + " " + NameHelper.GetRandomName());
                }
            }

            Loc.GetMap().GenerateMap();
        }
    }
}