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
        creature.Data.Thirst = Random.Range(35, 50);
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
            .GetCircle(new Coordinates(MapConstants.MapSize / 2, MapConstants.MapSize / 2), 10)
            .First(c => c.CellType != CellType.Water || c.CellType != CellType.Mountain);

        SummonCells(midCell);

        var firstCreature = SpawnCreature(midCell.GetNeighbor(Direction.E));

        midCell.AddContent(StructureController.Instance.GetStructure("Table").gameObject);

        CameraController.Instance.MoveToCell(firstCreature.Data.CurrentCell);

        var spawns = midCell.Neighbors.ToList();

        var waterCell = midCell.GetNeighbor(Direction.N);
        waterCell.CellType = CellType.Water;

        var foodCell = midCell.GetNeighbor(Direction.SE);
        for (var i = 0; i < 30; i++)
        {
            foodCell.AddContent(ItemController.Instance.GetItem("Apple").gameObject);
        }

        var woodCell = midCell.GetNeighbor(Direction.SW);
        for (var i = 0; i < 15; i++)
        {
            woodCell.AddContent(ItemController.Instance.GetItem("Rock").gameObject);
        }

        var rockCell = midCell.GetNeighbor(Direction.S);
        for (var i = 0; i < 15; i++)
        {
            rockCell.AddContent(ItemController.Instance.GetItem("Wood").gameObject);
        }

        //for (var i = 0; i < 2; i++)
        //{
        //    SpawnCreature(spawns[Random.Range(0, spawns.Count)]).Data.Speed = Random.Range(10, 15);
        //}
    }

    private static void SummonCells(CellData center)
    {
        center.CellType = CellType.Stone;
        MapGrid.Instance.BindCell(center, "X");

        foreach (var cell in center.Neighbors)
        {
            cell.CellType = CellType.Stone;
            MapGrid.Instance.BindCell(cell, "X");
        }

        GetRune(center.GetNeighbor(Direction.N).GetNeighbor(Direction.N));
        GetRune(center.GetNeighbor(Direction.E).GetNeighbor(Direction.E));
        GetRune(center.GetNeighbor(Direction.S).GetNeighbor(Direction.S));
        GetRune(center.GetNeighbor(Direction.W).GetNeighbor(Direction.W));
    }

    public static void GetRune(CellData location)
    {
        var rune = StructureController.Instance.GetStructure("BindRune");
        location.CellType = CellType.Stone;
        MapGrid.Instance.BindCell(location, rune.Data.GetGameId());
        location.AddContent(rune.gameObject);
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