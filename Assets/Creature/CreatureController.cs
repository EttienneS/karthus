using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    public Creature CreaturePrefab;

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
        creature.MoveToCell(spawnLocation);
        return creature;
    }


   
}
