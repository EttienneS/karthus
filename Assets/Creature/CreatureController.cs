using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreatureController : MonoBehaviour
{
    public CreatureRenderer CreaturePrefab;

    internal Dictionary<string, CreatureData> Beastiary = new Dictionary<string, CreatureData>();
    internal Dictionary<CreatureData, CreatureRenderer> CreatureLookup = new Dictionary<CreatureData, CreatureRenderer>();

    public CreatureRenderer GetCreatureAtPoint(Vector2 point)
    {
        foreach (var creature in CreatureLookup.Values)
        {
            var rect = new Rect(creature.transform.position.x - 0.5f, creature.transform.position.y - 0.5f, 1f, 1f);
            if (rect.Contains(point))
            {
                return creature;
            }
        }

        return null;
    }

    public void Awake()
    {
        foreach (var creatureFile in Game.FileController.CreatureFiles)
        {
            var creature = CreatureData.Load(creatureFile.text);
            Beastiary.Add(creature.Name, creature);
        }
    }

    internal void DestroyCreature(CreatureRenderer creature)
    {
        if (creature != null)
        {
            if (creature.Data.Task != null)
                creature.Data.Task.CancelTask();

            CreatureLookup.Remove(creature.Data);
            IdService.RemoveEntity(creature.Data);
            Game.Controller.AddItemToDestroy(creature.gameObject);
        }
    }

    internal CreatureData GetCreatureOfType(string v)
    {
        if (!Beastiary.ContainsKey(v))
        {
            Debug.Log($"Creature not found: {v}");
            throw new KeyNotFoundException();
        }

        return Beastiary[v].CloneJson();
    }

    internal CreatureRenderer GetCreatureForCreatureData(CreatureData creatureData)
    {
        return CreatureLookup[creatureData];
    }

    internal CreatureRenderer SpawnCreature(CreatureData creatureData, CellData cell, Faction faction)
    {
        var creature = Instantiate(CreaturePrefab, transform, true);
        creature.Data = creatureData;
        creature.Data.Cell = cell;
        creature.transform.position = cell.ToMapVector();
        creature.Data.WorkTick = Random.Range(0, Game.TimeManager.WorkInterval);
        creature.Data.GetBehaviourTask = Behaviours.GetBehaviourFor(creature.Data.BehaviourName);

        creature.MainRenderer.material = Game.MaterialController.DefaultMaterial;

        if (creatureData.Name == "Person")
        {
            creature.Data.ManaPool.InitColor(ManaColor.White, 10, 10, 10);
            creature.Data.ManaPool.InitColor(ManaColor.Red, 3, 10, 5);
            creature.Data.ManaPool.InitColor(ManaColor.Green, 3, 10, 5);
            creature.Data.ManaPool.InitColor(ManaColor.Blue, 0, 10, 0);
            creature.Data.ManaPool.InitColor(ManaColor.Black, 0, 10, 0);
        }
        else
        {
            creature.Data.ManaPool.InitColor(ManaColor.White, 10, 10, 10);
            creature.Data.ManaPool.InitColor(ManaColor.Red, 0, 10, 0);
            creature.Data.ManaPool.InitColor(ManaColor.Green, 0, 10, 0);
            creature.Data.ManaPool.InitColor(ManaColor.Blue, 0, 10, 0);
            creature.Data.ManaPool.InitColor(ManaColor.Black, 0, 10, 0);
        }

        IndexCreature(creature);
        faction.AddCreature(creatureData);
        return creature;
    }



    private void IndexCreature(CreatureRenderer creature)
    {
        CreatureLookup.Add(creature.Data, creature);
        IdService.EnrollEntity(creature.Data);

        creature.name = $"{creature.Data.Name} ({creature.Data.Id})";
    }
}