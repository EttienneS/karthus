using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreatureController : MonoBehaviour
{
    public CreatureRenderer CreaturePrefab;

    internal Dictionary<string, CreatureData> Beastiary = new Dictionary<string, CreatureData>();
    //internal Dictionary<CreatureData, CreatureRenderer> CreatureLookup = new Dictionary<CreatureData, CreatureRenderer>();

    public CreatureRenderer GetCreatureAtPoint(Vector2 point)
    {
        foreach (var creature in IdService.CreatureLookup.Values)
        {
            var rect = new Rect(creature.CreatureRenderer.transform.position.x - 0.5f, creature.CreatureRenderer.transform.position.y - 0.5f, 1f, 1f);
            if (rect.Contains(point))
            {
                return creature.CreatureRenderer;
            }
        }

        return null;
    }

    public void Awake()
    {
        foreach (var creatureFile in Game.FileController.CreatureFiles)
        {
            var creature = creatureFile.text.LoadJson<CreatureData>();
            Beastiary.Add(creature.Name, creature);
        }
    }

    internal void DestroyCreature(CreatureRenderer creature)
    {
        if (creature != null)
        {
            if (creature.Data.Task != null)
                creature.Data.CancelTask();

            Game.FactionController.Factions[creature.Data.FactionName].Creatures.Remove(creature.Data);
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

    public List<(CreatureData creature, Cell cell, Faction faction)> SpawnCache = new List<(CreatureData, Cell, Faction)>();

    public void Update()
    {
        foreach (var cached in SpawnCache)
        {
            SpawnCreature(cached.creature, cached.cell, cached.faction);
        }
        SpawnCache.Clear();
    }

    internal void CacheSpawn(CreatureData creatureData, Cell cell, Faction faction)
    {
        SpawnCache.Add((creatureData, cell, faction));
    }

    internal CreatureRenderer SpawnCreature(CreatureData creatureData, Cell cell, Faction faction)
    {
        var creature = Instantiate(CreaturePrefab, transform);
        creature.Data = creatureData;
        creature.Data.CreatureRenderer = creature;
        IdService.EnrollEntity(creature.Data);
        creature.name = $"{creature.Data.Name} ({creature.Data.Id})";

        creature.Data.Name = CreatureHelper.GetRandomName();
        creature.Data.Cell = cell;
        creature.transform.position = cell.ToMapVector();
        creature.Data.WorkTick = Random.Range(0, Game.TimeManager.WorkInterval);
        creature.Data.GetBehaviourTask = Behaviours.GetBehaviourFor(creature.Data.BehaviourName);

        creature.MainRenderer.material = Game.MaterialController.DefaultMaterial;

        creature.Data.ManaPool = new ManaPool(creature.Data);

        if (creatureData.BehaviourName == "Person")
        {
            creature.Data.ManaPool.InitColor(ManaColor.White, 10, 10, 10);
            creature.Data.ManaPool.InitColor(ManaColor.Red, 3, 10, 5);
            creature.Data.ManaPool.InitColor(ManaColor.Green, 3, 10, 5);
            creature.Data.ManaPool.InitColor(ManaColor.Blue, 0, 10, 0);
            creature.Data.ManaPool.InitColor(ManaColor.Black, 0, 10, 0);

            creature.EnableLight();
        }
        else if (creatureData.Name == "QueenWraith")
        {
            creature.Data.ManaPool.InitColor(ManaColor.White, 15, 15, 15);
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

        faction.AddCreature(creatureData);
        return creature;
    }
}