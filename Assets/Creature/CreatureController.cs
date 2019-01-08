using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    public Creature CreaturePrefab;

    public List<Creature> Creatures = new List<Creature>();
    private static CreatureController _instance;

    private float nextActionTime;

    private float period = 0.001f;

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

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextActionTime)
        {
            nextActionTime += period;
            foreach (var creature in Creatures)
            {
                creature.Act();
            }
        }
    }


}
