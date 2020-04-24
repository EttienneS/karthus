using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator
{
    public bool Done;
    public string Status;
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

  public float max = -100f;
  public float min = 100f;

    public Biome GetBiome(int x, int y)
    {
        //if (_biome == null)
        //{
        //    // _biome = BiomeTemplates.First(b => b.Name == "Default");
        //    _biome = BiomeTemplates.First(b => b.Name == "Mountain");
        //}
        //return _biome;
       
        var value = Game.Instance.Map.WorldNoiseMap[x, y];

        if (value > max)
        {
            max = value;
        }
        if (value < min)
        {
            min = value;
        }

        if (value > 0.65f)
        {
            return BiomeTemplates["Mountain"];
        }
        if (value < 0.25f)
        {
            return BiomeTemplates["Water"];
        }
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
            if (monster.Key == "Skeleton" || monster.Key == "Person")
            {
                continue;
            }

            for (int i = 0; i < 3; i++)
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

    public void Work()
    {
        using (Instrumenter.Init())
        {
            Game.Instance.Map.Chunks = new Dictionary<(int x, int y), ChunkRenderer>();

            if (SaveManager.SaveToLoad == null)
            {
                for (var i = 0; i < Game.Instance.Map.Size; i++)

                {
                    for (var k = 0; k < Game.Instance.Map.Size; k++)
                    {
                        Game.Instance.Map.MakeChunk(new Chunk((Game.Instance.Map.Origin.X / Game.Instance.Map.ChunkSize) + i,
                                                      (Game.Instance.Map.Origin.Y / Game.Instance.Map.ChunkSize) + k));
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

            Done = true;
        }

    }

    public IEnumerator xWork()
    {
        Game.Instance.Map.Chunks = new Dictionary<(int x, int y), ChunkRenderer>();

        var counter = 1f;
        var total = Game.Instance.Map.Size * Game.Instance.Map.Size * 1f;
        Game.Instance.SetLoadStatus("Create Map", 0);
        if (SaveManager.SaveToLoad == null)
        {
            for (var i = 0; i < Game.Instance.Map.Size; i++)
            {
                for (var k = 0; k < Game.Instance.Map.Size; k++)
                {
                    Game.Instance.Map.MakeChunk(new Chunk((Game.Instance.Map.Origin.X / Game.Instance.Map.ChunkSize) + i,
                                                 (Game.Instance.Map.Origin.Y / Game.Instance.Map.ChunkSize) + k));

                    Game.Instance.SetLoadStatus($"Create Chunk {counter}", counter / total);
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
                Game.Instance.SetLoadStatus($"Load Chunk {counter}", step * counter);
                yield return null;
            }
        }

        Done = true;
    }
}