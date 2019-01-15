using System.Collections.Generic;
using UnityEngine;

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

        transform.position = spawnLocation.transform.position;
        spawnLocation.AddCreature(creature);
        creature.See();

        Creatures.Add(creature);
        return creature;
    }

    public void SpawnCreatures()
    {
        var firstCreature = SpawnCreature(MapGrid.Instance.GetRandomPathableCell());
        firstCreature.Speed = Random.Range(10, 15);
        CameraController.Instance.MoveToCell(firstCreature.CurrentCell);

        for (int i = 0; i < CreaturesToSpawn - 1; i++)
        {
            SpawnCreature(MapGrid.Instance.GetRandomPathableCell()).Speed = Random.Range(10, 15);
        }


    }

}