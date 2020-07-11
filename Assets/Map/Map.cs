using Assets.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Map : MonoBehaviour
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
                foreach (var biomeFile in Game.Instance.FileController.BiomeFiles)
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
            var x = Game.Instance.MapData.Size * Game.Instance.MapData.ChunkSize / 2;
            var z = Game.Instance.MapData.Size * Game.Instance.MapData.ChunkSize / 2;
            return CellLookup[(x, z)];
        }
    }

    public float[,] LocalNoiseMap
    {
        get
        {
            if (_localNoiseMap == null)
            {
                _localNoiseMap = Noise.GenerateNoiseMap(SeedValue, Game.Instance.MaxSize, Game.Instance.MaxSize, LocalNoise, Vector2.zero);
            }
            return _localNoiseMap;
        }
    }

    internal int MaxX => Game.Instance.MaxSize;
    internal int MaxZ => Game.Instance.MaxSize;
    internal int MinX => 0;
    internal int MinZ => 0;

    internal int SeedValue
    {
        get
        {
            if (!_seedValue.HasValue)
            {
                var md5Hasher = MD5.Create();
                var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(Game.Instance.MapData.Seed));
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
        var noiseMapHeight = Game.Instance.Map.GetNoiseMapPoint(x, z);
        var biome = Game.Instance.Map.GetBiome(x, z).GetRegion(noiseMapHeight);

        var y = Game.Instance.MapData.StructureLevel;

        var cell = new Cell
        {
            X = x,
            BiomeRegion = biome,
            Y = y,
            Z = z,
            SearchPhase = 0
        };

        Game.Instance.Map.CellLookup.Add((x, z), cell);

        return cell;
    }

    public void GenerateMap()
    {
        var size = Game.Instance.MaxSize;
        MakeCells(size);
        Game.Instance.Map.Chunks = new Dictionary<(int x, int y), ChunkRenderer>();

        if (SaveManager.SaveToLoad == null)
        {
            for (var x = 0; x < Game.Instance.MapData.Size; x++)
            {
                for (var y = 0; y < Game.Instance.MapData.Size; y++)
                {
                    Game.Instance.Map.MakeChunk(new Chunk(x, y));
                }
            }
            PopulateCells();
        }
        else
        {
            foreach (var chunk in SaveManager.SaveToLoad.Chunks)
            {
                Game.Instance.Map.MakeChunk(chunk);
            }
        }
    }

    public Biome GetBiome(int x, int y)
    {
        //return BiomeTemplates["Debug"];
        return BiomeTemplates["Default"];
    }

    public Cell GetCellAtCoordinate((float x, float y) coords)
    {
        return GetCellAtCoordinate(coords.x, coords.y);
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
        var (minx, maxx, minz, maxz) = Game.Instance.Map.GetMinMax(square);

        return (Game.Instance.Map.GetCellAtCoordinate(minx, minz),
                Game.Instance.Map.GetCellAtCoordinate(maxx, minz),
                Game.Instance.Map.GetCellAtCoordinate(minx, maxz),
                Game.Instance.Map.GetCellAtCoordinate(maxx, maxz));
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

    public float GetNoiseMapPoint(float x, float y)
    {
        return LocalNoiseMap[(int)x, (int)y];
    }

    public ChunkRenderer MakeChunk(Chunk data)
    {
        var chunk = Instantiate(ChunkPrefab, transform);
        chunk.transform.position = new Vector2(data.X * Game.Instance.MapData.ChunkSize, data.Z * Game.Instance.MapData.ChunkSize);
        chunk.name = $"Chunk: {data.X}_{data.Z}";
        chunk.Data = data;

        Game.Instance.Map.Chunks.Add((data.X, data.Z), chunk);

        return chunk;
    }

    public void MakeFactionBootStrap(Faction faction)
    {
        var center = Game.Instance.Map.GetNearestPathableCell(Game.Instance.Map.Center, Mobility.Walk, 10);

        Game.Instance.FactionController.PlayerFaction.HomeCells.AddRange(Game.Instance.Map.GetCircle(Game.Instance.Map.Center, 15));

        var open = Game.Instance.Map.GetCircle(center, 5).Where(c => c.Pathable(Mobility.Walk) && c.Structure == null);
        Game.Instance.ItemController.SpawnItem("Berries", open.GetRandomItem(), 250);
        Game.Instance.ItemController.SpawnItem("Wood", open.GetRandomItem(), 250);
        Game.Instance.ItemController.SpawnItem("Stone", open.GetRandomItem(), 250);

        for (int i = 0; i < Game.Instance.MapData.CreaturesToSpawn; i++)
        {
            var c = Game.Instance.CreatureController.SpawnCreature(Game.Instance.CreatureController.GetCreatureOfType("Person"),
                                                                   Game.Instance.Map.GetNearestPathableCell(center, Mobility.Walk, 10),
                                                                   faction);
        }
    }

    public void SpawnCreatures()
    {
        foreach (var monster in Game.Instance.CreatureController.Beastiary)
        {
            if (monster.Key == "Person")
            {
                continue;
            }

            for (int i = 0; i < Game.Instance.MapData.CreaturesToSpawn; i++)
            {
                var creature = Game.Instance.CreatureController.GetCreatureOfType(monster.Key);

                var spot = Game.Instance.Map.GetCircle(Game.Instance.Map.Center, 25).GetRandomItem();
                if (spot.TravelCost <= 0 && creature.Mobility != Mobility.Fly)
                {
                    spot = Game.Instance.Map.CellLookup.Values.Where(c => c.TravelCost > 0).GetRandomItem();
                }
                Game.Instance.CreatureController.SpawnCreature(creature, spot, Game.Instance.FactionController.MonsterFaction);
            }
        }
    }

    internal Cell GetCellAtCoordinate(Vector3 pos)
    {
        return GetCellAtCoordinate(pos.x, pos.z);
    }

    internal Cell GetCellAtCoordinate(Vector2 pos)
    {
        return GetCellAtCoordinate(pos.x, pos.y);
    }

    internal Cell GetCellAttRadian(Cell center, int radius, int angle)
    {
        var mineX = Mathf.Clamp(Mathf.FloorToInt(center.X + (radius * Mathf.Cos(angle))), 0, MaxX);
        var mineY = Mathf.Clamp(Mathf.FloorToInt(center.Z + (radius * Mathf.Sin(angle))), 0, MaxZ);

        return GetCellAtCoordinate(mineX, mineY);
    }

    internal Cell GetNearestEmptyCell(Cell cell)
    {
        var circle = GetCircle(cell, 1);

        for (int i = 2; i < 15; i++)
        {
            var newCircle = GetCircle(cell, i);
            newCircle.RemoveAll(c => circle.Contains(c));
            circle = newCircle;

            var empty = circle.Where(c => c.Structure == null);

            if (empty.Count() > 0)
            {
                return empty.GetRandomItem();
            }
        }

        return null;
    }

    internal Cell GetNearestPathableCell(Cell centerPoint, Mobility mobility, int radius)
    {
        var circle = GetCircle(centerPoint, radius);

        return circle.Where(c => c != centerPoint && c.Pathable(mobility))
                           .OrderBy(c => c.DistanceTo(centerPoint))
                           .First();
    }

    internal void MakeCells(int size)
    {
        using (Instrumenter.Start())
        {
            Game.Instance.Map.Cells = new List<Cell>();
            for (var y = 0; y < 0 + size; y++)
            {
                for (var x = 0; x < 0 + size; x++)
                {
                    Game.Instance.Map.Cells.Add(CreateCell(x, y));
                }
            }

            // link cell to the others in the chunk
            for (var z = 0; z < 0 + size; z++)
            {
                for (var x = 0; x < 0 + size; x++)
                {
                    var cell = Game.Instance.Map.CellLookup[(x, z)];
                    if (x > 0)
                    {
                        cell.SetNeighbor(Direction.W, Game.Instance.Map.CellLookup[(x - 1, z)]);

                        if (z > 0)
                        {
                            cell.SetNeighbor(Direction.SW, Game.Instance.Map.CellLookup[(x - 1, z - 1)]);

                            if (x < size - 1)
                            {
                                cell.SetNeighbor(Direction.SE, Game.Instance.Map.CellLookup[(x + 1, z - 1)]);
                            }
                        }
                    }

                    if (z > 0)
                    {
                        cell.SetNeighbor(Direction.S, Game.Instance.Map.CellLookup[(x, z - 1)]);
                    }
                }
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
            if (Game.Instance.MapData.Populate)
            {
                foreach (var cell in Game.Instance.Map.Cells)
                {
                    cell.Populate();
                }

                MakeFactionBootStrap(Game.Instance.FactionController.PlayerFaction);
                SpawnCreatures();
            }
        }
    }
}