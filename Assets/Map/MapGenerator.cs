using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MapGenerator
{
    public Biome Biome;

    public bool Busy;
    public bool Done;
    public string Status;
    private List<Biome> _biomeTemplates;

    public List<Biome> BiomeTemplates
    {
        get
        {
            if (_biomeTemplates == null)
            {
                _biomeTemplates = new List<Biome>();
                foreach (var biomeFile in Game.FileController.BiomeFiles)
                {
                    _biomeTemplates.Add(biomeFile.text.LoadJson<Biome>());
                }
            }

            return _biomeTemplates;
        }
    }

    public (Cell bottomLeft, Cell bottomRight, Cell topLeft, Cell topRight) GetCorners(List<Cell> square)
    {
        var minMax = Game.Map.GetMinMax(square);

        return (Game.Map.GetCellAtCoordinate(minMax.minx, minMax.miny),
                Game.Map.GetCellAtCoordinate(minMax.maxx, minMax.miny),
                Game.Map.GetCellAtCoordinate(minMax.minx, minMax.maxy),
                Game.Map.GetCellAtCoordinate(minMax.maxx, minMax.maxy));
    }

    public void MakeFactionBootStrap(Faction faction)
    {
        var center = Game.Map.GetNearestPathableCell(Game.Map.Center, Mobility.Walk, 25);

        Game.FactionController.PlayerFaction.HomeCells.AddRange(Game.Map.GetCircle(Game.Map.Center, 15));

        var open = Game.Map.GetCircle(center, 5).Where(c => c.Pathable(Mobility.Walk) && c.Structure == null);
        Game.ItemController.SpawnItem("Berries", open.GetRandomItem(), 250);
        Game.ItemController.SpawnItem("Wood", open.GetRandomItem(), 250);
        Game.ItemController.SpawnItem("Stone", open.GetRandomItem(), 250);

        for (int i = 0; i < 1; i++)
        {
            var c = Game.CreatureController.SpawnCreature(Game.CreatureController.GetCreatureOfType("Person"),
                                                          Game.Map.GetNearestPathableCell(center, Mobility.Walk, 5),
                                                          faction);
        }
    }

    public void SpawnCreatures()
    {
        foreach (var monster in Game.CreatureController.Beastiary)
        {
            if (monster.Key == "Person")
            {
                continue;
            }

            for (int i = Game.Map.MinY; i < Game.Map.MaxX / 50; i++)
            {
                var creature = Game.CreatureController.GetCreatureOfType(monster.Key);

                var spot = Game.Map.GetRandomCell();
                if (spot.TravelCost <= 0 && creature.Mobility != Mobility.Fly)
                {
                    spot = Game.Map.CellLookup.Values.Where(c => c.TravelCost > 0).GetRandomItem();
                }
                Game.CreatureController.SpawnCreature(creature, spot, Game.FactionController.MonsterFaction);
            }
        }
    }

    public IEnumerator Work()
    {
        Biome = BiomeTemplates.First(b => b.Name == "Default");

        Game.Map.Chunks = new Dictionary<(int x, int y), ChunkRenderer>();

        var counter = 1;
        Game.Instance.SetLoadStatus("Create Map", 0);
        var size = 1;
        var inc = 1f / 9f;
        if (SaveManager.SaveToLoad == null)
        {
            for (var i = 0 - size; i <= 0 + size; i++)
            {
                for (var k = 0 - size; k <= 0 + size; k++)
                {
                    Game.Map.MakeChunk(new Chunk((Game.Map.Origin.X / Game.Map.ChunkSize) + i,
                                                 (Game.Map.Origin.Y / Game.Map.ChunkSize) + k));

                    Game.Instance.SetLoadStatus($"Create Chunk {counter}", (counter - 1) * inc);
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
                Game.Map.MakeChunk(chunk);
                Game.Instance.SetLoadStatus($"Load Chunk {counter}", step * counter);
                yield return null;
            }
        }

        Done = true;
    }
}