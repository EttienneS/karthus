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

        transform.position = new Vector3(spawnLocation.transform.position.x, spawnLocation.transform.position.y, -0.25f);
        spawnLocation.AddCreature(creature);
        creature.See();

        Creatures.Add(creature);
        return creature;
    }

    public void SpawnCreatures()
    {
        SpawnCreature(MapGrid.Instance.GetRandomPathableCell()).Speed = 1;

        CameraController.Instance.MoveToCell(SpawnCreature(MapGrid.Instance.GetRandomPathableCell()).CurrentCell);

        SpawnCreature(MapGrid.Instance.GetRandomPathableCell()).Speed = 10;
    }

    private void Update()
    {
        deltaTime += Time.deltaTime;
        if (deltaTime > ActPeriod)
        {
            deltaTime = 0;
            foreach (var creature in Creatures)
            {
                if (creature.Task == null)
                {
                    creature.Task = Taskmaster.Instance.GetTask(creature);
                    creature.DoTask();
                }
            }
        }
    }
}