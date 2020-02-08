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

        var open = Game.Map.GetCircle(center, 8).Where(c => c.Pathable(Mobility.Walk));
        Game.ItemController.SpawnItem("Berries", open.GetRandomItem(), 100);
        Game.ItemController.SpawnItem("Wood", open.GetRandomItem(), 25);
        Game.ItemController.SpawnItem("Stone", open.GetRandomItem(), 25);

        for (int i = 0; i < 3; i++)
        {
            var c = Game.CreatureController.SpawnCreature(Game.CreatureController.GetCreatureOfType("Person"),
                                                     Game.Map.GetNearestPathableCell(center, Mobility.Walk, 25),
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

        Game.Instance.SetLoadStatus("Create initial chunks", 0.08f);
        MakeChunks(Game.Map.MinX, Game.Map.MinY, Game.Map.MaxX, Game.Map.MaxY);
        Game.Instance.SetLoadStatus("Create cells", 0.18f);
        yield return null;

        var i = 0f;
        var totalProgress = 0.3f;
        var totalChunks = Game.Map.Chunks.Keys.Count;
        foreach (var chunk in Game.Map.Chunks.Values)
        {
            i++;
            Game.Instance.SetLoadStatus($"Draw {i}/{totalChunks}", 0.5f + (i / totalChunks * totalProgress));
            chunk.Populate();
            chunk.Draw();
            yield return null;
        }

        Done = true;
    }

    private void MakeChunks(int minX, int minY, int maxX, int maxY)
    {
        var size = Game.Map.ChunkSize;

        var startX = minX / size;
        var startY = minY / size;

        var wchunks = (maxX - minX) / size;
        var hchunks = (maxY - minY) / size;

        var currentX = startX;
        var currentY = startY;

        Game.Map.Chunks = new Dictionary<(int x, int y), Chunk>();
        for (int y = 0; y < wchunks; y++)
        {
            for (int x = 0; x < hchunks; x++)
            {
                var chunk = Game.Map.MakeChunk(currentX, currentY);
                Game.Map.Chunks.Add((currentX, currentY), chunk);

                currentX++;
            }
            currentX = startX;
            currentY++;
        }
    }
}