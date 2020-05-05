﻿using Assets.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MapGenerator
{
    private Dictionary<string, Biome> _biomeTemplates;

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

    public Biome GetBiome(int x, int y)
    {
        //if (_biome == null)
        //{
        //    // _biome = BiomeTemplates.First(b => b.Name == "Default");
        //    _biome = BiomeTemplates.First(b => b.Name == "Mountain");
        //}
        //return _biome;

        //var value = Game.Instance.Map.WorldNoiseMap[x, y];

        //if (value > 0.65f)
        //{
        //    return BiomeTemplates["Mountain"];
        //}
        //if (value < 0.25f)
        //{
        //    return BiomeTemplates["Water"];
        //}
        return BiomeTemplates["Default"];
    }

    public (Cell bottomLeft, Cell bottomRight, Cell topLeft, Cell topRight) GetCorners(List<Cell> square)
    {
        var minMax = Game.Instance.Map.GetMinMax(square);

        return (Game.Instance.Map.GetCellAtCoordinate(minMax.minx, minMax.miny),
                Game.Instance.Map.GetCellAtCoordinate(minMax.maxx, minMax.miny),
                Game.Instance.Map.GetCellAtCoordinate(minMax.minx, minMax.maxy),
                Game.Instance.Map.GetCellAtCoordinate(minMax.maxx, minMax.maxy));
    }

    public void MakeFactionBootStrap(Faction faction)
    {
        var center = Game.Instance.Map.GetNearestPathableCell(Game.Instance.Map.Center, Mobility.Walk, 25);

        Game.Instance.FactionController.PlayerFaction.HomeCells.AddRange(Game.Instance.Map.GetCircle(Game.Instance.Map.Center, 15));

        var open = Game.Instance.Map.GetCircle(center, 10).Where(c => c.Pathable(Mobility.Walk) && c.Structure == null);
        Game.Instance.ItemController.SpawnItem("Berries", open.GetRandomItem(), 250);
        Game.Instance.ItemController.SpawnItem("Wood", open.GetRandomItem(), 250);
        Game.Instance.ItemController.SpawnItem("Stone", open.GetRandomItem(), 250);

        for (int i = 0; i < Game.Instance.Map.CreaturesToSpawn; i++)
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
            continue;

            if (monster.Key == "Person")
            {
                continue;
            }

            for (int i = 0; i < Game.Instance.Map.CreaturesToSpawn; i++)
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

    public Cell CreateCell(int x, int y)
    {
        var cell = new Cell
        {
            X = x,
            Y = y,
            SearchPhase = 0
        };

        Game.Instance.Map.CellLookup.Add((x, y), cell);

        return cell;
    }

    internal void MakeCells(int size)
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
        for (var y = 0; y < 0 + size; y++)
        {
            for (var x = 0; x < 0 + size; x++)
            {
                var cell = Game.Instance.Map.CellLookup[(x, y)];
                if (x > 0)
                {
                    cell.SetNeighbor(Direction.W, Game.Instance.Map.CellLookup[(x - 1, y)]);

                    if (y > 0)
                    {
                        cell.SetNeighbor(Direction.SW, Game.Instance.Map.CellLookup[(x - 1, y - 1)]);

                        if (x < 0 - 1)
                        {
                            cell.SetNeighbor(Direction.SE, Game.Instance.Map.CellLookup[(x + 1, y - 1)]);
                        }
                    }
                }

                if (y > 0)
                {
                    cell.SetNeighbor(Direction.S, Game.Instance.Map.CellLookup[(x, y - 1)]);
                }
            }
        }
    }

    public void GenerateMap()
    {
        using (Instrumenter.Start())
        {
            var size = Game.Instance.Map.ChunkSize * Game.Instance.Map.Size;
            MakeCells(size);
            Game.Instance.Map.Chunks = new Dictionary<(int x, int y), ChunkRenderer>();

            if (SaveManager.SaveToLoad == null)
            {
                for (var x = 0; x < Game.Instance.Map.Size; x++)
                {
                    for (var y = 0; y < Game.Instance.Map.Size; y++)
                    {
                        Game.Instance.Map.MakeChunk(new Chunk(x, y));
                    }
                }
            }
            else
            {
                foreach (var chunk in SaveManager.SaveToLoad.Chunks)
                {
                    Game.Instance.Map.MakeChunk(chunk);
                }
            }
        }
    }

    public IEnumerator Work()
    {
        Game.Instance.Map.Chunks = new Dictionary<(int x, int y), ChunkRenderer>();

        var counter = 1f;
        var total = Game.Instance.Map.Size * Game.Instance.Map.Size * 1f;
        if (SaveManager.SaveToLoad == null)
        {
            for (var x = 0; x < Game.Instance.Map.Size; x++)
            {
                for (var y = 0; y < Game.Instance.Map.Size; y++)
                {
                    Game.Instance.Map.MakeChunk(new Chunk(x, y));
                    counter++;
                    yield return null;
                }
            }
        }
        else
        {
            var step = 1f / SaveManager.SaveToLoad.Chunks.Count;
            foreach (var chunk in SaveManager.SaveToLoad.Chunks)
            {
                counter++;
                Game.Instance.Map.MakeChunk(chunk);
                yield return null;
            }
        }
    }
}