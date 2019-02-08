using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreatureController : MonoBehaviour
{
    [Range(0.01f, 2f)]
    public float ActPeriod = 1f;

    public Creature CreaturePrefab;

    public List<Creature> Creatures = new List<Creature>();

    [Range(1, 1000)]
    public int CreaturesToSpawn = 10;

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

    public Creature SpawnCreature(Cell spawnLocation)
    {
        var creature = Instantiate(CreaturePrefab, transform, true);
        creature.Data.Name = CreatureHelper.GetRandomName();

        creature.transform.position = spawnLocation.transform.position;
        creature.Data.Coordinates = spawnLocation.Data.Coordinates;

        IndexCreature(creature);
        return creature;
    }

    private void IndexCreature(Creature creature)
    {
        Creatures.Add(creature);
        CreatureLookup.Add(creature.Data, creature);
    }

    internal void DestroyCreature(Creature creature)
    {
        CreatureLookup.Remove(creature.Data);

        Destroy(creature.gameObject);
    }

    public void SpawnCreatures()
    {
        var firstCreature = SpawnCreature(MapGrid.Instance.GetRandomPathableCell());
        firstCreature.Data.Speed = Random.Range(10, 15);
        CameraController.Instance.MoveToCell(firstCreature.Data.CurrentCell.LinkedGameObject);

        // spawn creatures in a circle around the 'first' one
        var spawns = MapGrid.Instance.GetCircle(firstCreature.Data.CurrentCell.LinkedGameObject, 3).Where(c => c.TravelCost > 0).ToList();
        var spawnCount = spawns.Count;
        for (int i = 0; i < spawnCount * 2; i++)
        {
            spawns[Random.Range(0, spawnCount)].AddContent(ItemController.Instance.GetItem("Apple").gameObject, true);
            spawns[Random.Range(0, spawnCount)].AddContent(ItemController.Instance.GetItem("Wood").gameObject, true);
        }

        for (int i = 0; i < CreaturesToSpawn - 1; i++)
        {
            SpawnCreature(spawns[Random.Range(0, spawns.Count)]).Data.Speed = Random.Range(10, 15);
        }
    }

    internal Creature LoadCreature(CreatureData savedCreature)
    {
        var creature = Instantiate(CreaturePrefab, transform, true);
        creature.Data = savedCreature;

        creature.transform.position = MapGrid.Instance.GetCellAtCoordinate(savedCreature.Coordinates).transform.position;
        creature.GetSprite();
        IndexCreature(creature);
        return creature;
    }

    internal Creature GetCreatureForCreatureData(CreatureData creatureData)
    {
        return CreatureLookup[creatureData];
    }
}