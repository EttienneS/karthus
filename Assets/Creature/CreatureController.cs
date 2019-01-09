using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    [Range(0.001f, 0.1f)]
    public float ActPeriod = 0.01f;

    public Creature CreaturePrefab;

    public List<Creature> Creatures = new List<Creature>();
    private static CreatureController _instance;

    private float nextActionTime;
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
        creature.MoveToCell(spawnLocation);

        Creatures.Add(creature);
        return creature;
    }

    public void SpawnCreatures()
    {
        for (var i = 0; i < 5; i++)
        {
            SpawnCreature(MapGrid.Instance.GetRandomPathableCell());
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        if (Time.time > nextActionTime)
        {
            nextActionTime += ActPeriod;
            foreach (var creature in Creatures)
            {
                creature.Act();
            }
        }
    }
}