using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreatureController : MonoBehaviour
{
    public Creature CreaturePrefab;

    internal List<Creature> Creatures = new List<Creature>();

    internal Dictionary<int, CreatureData> CreatureIdLookup = new Dictionary<int, CreatureData>();
    internal Dictionary<CreatureData, Creature> CreatureLookup = new Dictionary<CreatureData, Creature>();
    private static CreatureController _instance;

    public static CreatureController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("CreatureController").GetComponent<CreatureController>();
            }

            return _instance;
        }
    }

    public Creature GetCreatureAtPoint(Vector2 point)
    {
        foreach (var creature in Creatures)
        {
            var rect = new Rect(creature.transform.position.x - 0.5f, creature.transform.position.y - 0.5f, 1f, 1f);
            if (rect.Contains(point))
            {
                return creature;
            }
        }

        return null;
    }

    public Creature SpawnCreature(CellData spawnLocation)
    {
        var creature = Instantiate(CreaturePrefab, transform, true);
        creature.Data.Name = CreatureHelper.GetRandomName();

        creature.transform.position = spawnLocation.Coordinates.ToMapVector();
        creature.Data.Coordinates = spawnLocation.Coordinates;

        creature.Data.Id = Creatures.Count + 1;

        creature.Data.Hunger = Random.Range(0, 15);
        creature.Data.Thirst = Random.Range(0, 15);
        creature.Data.Energy = Random.Range(80, 100);

        creature.Data.SpriteId = Random.Range(0, SpriteStore.Instance.CreatureSprite.Keys.Count - 1);

        creature.GetSprite();

        creature.ShowText("Awee!!!", Random.Range(1f, 3f));

        IndexCreature(creature);
        return creature;
    }

    public void SpawnCreatures()
    {
        var midCell = MapGrid.Instance
            .GetCircle(new Coordinates(Constants.MapSize / 2, Constants.MapSize / 2), 10)
            .First(c => c.CellType != CellType.Water || c.CellType != CellType.Mountain);

        SummonCells(midCell);

        var firstCreature = SpawnCreature(midCell);

        CameraController.Instance.MoveToCell(firstCreature.Data.CurrentCell);

        // spawn creatures in a circle around the 'first' one
        //var spawns = MapGrid.Instance.GetCircle(firstCreature.Data.CurrentCell.Coordinates, 3).Where(c => c.TravelCost == 1 && c.Structure == null).ToList();

        //var foodCell = spawns[Random.Range(0, spawns.Count)];
        //for (var i = 0; i < 30; i++)
        //{
        //    foodCell.AddContent(ItemController.Instance.GetItem("Apple").gameObject);
        //}

        //var woodCell = spawns[Random.Range(0, spawns.Count)];
        //for (var i = 0; i < 15; i++)
        //{
        //    woodCell.AddContent(ItemController.Instance.GetItem("Rock").gameObject);
        //}

        //var rockCell = spawns[Random.Range(0, spawns.Count)];
        //for (var i = 0; i < 15; i++)
        //{
        //    rockCell.AddContent(ItemController.Instance.GetItem("Wood").gameObject);
        //}

        //for (var i = 0; i < 2; i++)
        //{
        //    SpawnCreature(spawns[Random.Range(0, spawns.Count)]).Data.Speed = Random.Range(10, 15);
        //}
    }

    public static void GetRune(int number, CellData location)
    {
        var rune = StructureController.Instance.GetStructure(new StructureData("rune", $"runeBlue_slab_00{number}"));
        rune.Data.Behaviour = new Bind(rune.Data.GetGameId(), location.Coordinates, 6, 30, 0.5f);

        MapGrid.Instance.BindCell(location, rune.Data.GetGameId());

        if (location.Structure != null)
        {
            StructureController.Instance.DestroyStructure(location.Structure);
        }
        location.AddContent(rune.gameObject);
    }

    private static void SummonCells(CellData center)
    {
        MapGrid.Instance.BindCell(center, "X");
        //var summonArea = MapGrid.Instance.GetCircle(center.Coordinates, 8);
        //summonArea = MapGrid.Instance.BleedGroup(summonArea, 3, 0.4f);

        GetRune(1, center.GetNeighbor(Direction.N).GetNeighbor(Direction.N));
        GetRune(2, center.GetNeighbor(Direction.E).GetNeighbor(Direction.E));
        GetRune(3, center.GetNeighbor(Direction.S).GetNeighbor(Direction.S));
        GetRune(4, center.GetNeighbor(Direction.W).GetNeighbor(Direction.W));

        //var redraws = new HashSet<Texture2D>();
        //foreach (var cell in summonArea)
        //{
        //    redraws.Add(MapGrid.Instance.SummonCell(cell));
        //}

        //foreach (var redraw in redraws)
        //{
        //
        //}
    }

    internal void DestroyCreature(Creature creature)
    {
        CreatureLookup.Remove(creature.Data);

        Destroy(creature.gameObject);
    }

    internal Creature GetCreatureForCreatureData(CreatureData creatureData)
    {
        return CreatureLookup[creatureData];
    }

    internal Creature LoadCreature(CreatureData savedCreature)
    {
        var creature = Instantiate(CreaturePrefab, transform, true);
        creature.Data = savedCreature;

        creature.transform.position = savedCreature.Coordinates.ToMapVector();
        creature.GetSprite();
        IndexCreature(creature);
        return creature;
    }

    private void IndexCreature(Creature creature)
    {
        creature.SpriteRenderer.sortingOrder = creature.Data.Id;
        Creatures.Add(creature);
        CreatureLookup.Add(creature.Data, creature);
        CreatureIdLookup.Add(creature.Data.Id, creature.Data);

        creature.name = $"{creature.Data.Name} ({creature.Data.Id})";
    }
}