using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    [Range(0.01f, 2f)]
    public float ActPeriod = 1f;

    public Creature CreaturePrefab;

    public List<Creature> Creatures = new List<Creature>();
    private static CreatureController _instance;

    private float deltaTime = 0;

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
        var creature = SpawnCreature(MapGrid.Instance.GetRandomPathableCell());
        CameraController.Instance.MoveToCell(creature.CurrentCell);
    }
    private void Update()
    {
        deltaTime += Time.deltaTime;
        if (deltaTime > ActPeriod)
        {
            deltaTime = 0;
            nextActionTime += ActPeriod;
            foreach (var creature in Creatures)
            {
                creature.Act();
            }
        }
    }
}