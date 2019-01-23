﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreatureController : MonoBehaviour
{
    [Range(0.01f, 2f)]
    public float ActPeriod = 1f;

    [Range(1, 1000)]
    public int CreaturesToSpawn = 10;

    public Creature CreaturePrefab;

    public List<Creature> Creatures = new List<Creature>();
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

    public Creature SpawnCreature(Cell spawnLocation)
    {
        var creature = Instantiate(CreaturePrefab, transform, true);

        creature.transform.position = spawnLocation.transform.position;
        spawnLocation.AddCreature(creature);

        Creatures.Add(creature);
        return creature;
    }

    public void SpawnCreatures()
    {
        var firstCreature = SpawnCreature(MapGrid.Instance.GetRandomPathableCell());
        firstCreature.Speed = Random.Range(10, 15);
        CameraController.Instance.MoveToCell(firstCreature.CurrentCell);

        // spawn creatures in a circle around the 'first' one
        var spawns = MapGrid.Instance.GetCircle(firstCreature.CurrentCell, 3).Where(c => c.TravelCost > 0).ToList();

        for (int i = 0; i < CreaturesToSpawn - 1; i++)
        {
            SpawnCreature(spawns[Random.Range(0, spawns.Count)]).Speed = Random.Range(10, 15);
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
}