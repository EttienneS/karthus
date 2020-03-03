using LPC.Spritesheet.Generator.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreatureController : MonoBehaviour
{
    public CreatureRenderer CreaturePrefab;

    internal Dictionary<string, Creature> Beastiary = new Dictionary<string, Creature>();

    public CreatureRenderer GetCreatureAtPoint(Vector2 point)
    {
        foreach (var creature in Game.IdService.CreatureLookup.Values)
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
            try
            {
                var creature = creatureFile.text.LoadJson<Creature>();
                Beastiary.Add(creature.Name, creature);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unable to load creature {creatureFile}: {ex.Message}");
            }
        }
    }

    internal void DestroyCreature(CreatureRenderer creature)
    {
        if (creature != null)
        {
            Debug.Log($"Destroying: {creature.Data.Name}");
            if (creature.Data.Task != null)
                creature.Data.CancelTask();

            Game.FactionController.Factions[creature.Data.FactionName].Creatures.Remove(creature.Data);
            Game.IdService.RemoveEntity(creature.Data);
            Game.Instance.AddItemToDestroy(creature.gameObject);
        }
    }

    internal Creature GetCreatureOfType(string v)
    {
        if (!Beastiary.ContainsKey(v))
        {
            Debug.Log($"Creature not found: {v}");
            throw new KeyNotFoundException();
        }

        return Beastiary[v].CloneJson();
    }

    public List<(Creature creature, Cell cell, Faction faction)> SpawnCache = new List<(Creature, Cell, Faction)>();

    public void Update()
    {
        foreach (var cached in SpawnCache)
        {
            SpawnCreature(cached.creature, cached.cell, cached.faction);
        }
        SpawnCache.Clear();
    }

    internal void CacheSpawn(Creature creatureData, Cell cell, Faction faction)
    {
        SpawnCache.Add((creatureData, cell, faction));
    }

    internal CreatureRenderer SpawnCreature(Creature creatureData, Cell cell, Faction faction)
    {
        var creature = Instantiate(CreaturePrefab, transform);
        creature.Data = creatureData;
        creature.Data.CreatureRenderer = creature;
        Game.IdService.EnrollEntity(creature.Data);
        creature.name = $"{creature.Data.Name} ({creature.Data.Id})";

        creature.Data.Gender = Random.value > 0.5f ? Gender.Male : Gender.Female;
        creature.Data.Name = NameHelper.GetRandomName();
        creature.Data.X = cell.Vector.x + Random.Range(-0.25f, 0.25f);
        creature.Data.Y = cell.Vector.y + Random.Range(-0.25f, 0.25f);
        creature.UpdatePosition();

        creature.Data.InternalTick = Random.Range(0, Game.TimeManager.CreatureTick);

        creature.Data.GetBehaviourTask = Behaviours.GetBehaviourFor(creature.Data.BehaviourName);

        creature.Data.Needs = Behaviours.GetNeedsFor(creature.Data.BehaviourName);

        creature.MainRenderer.color = Color.white;
        faction.AddCreature(creatureData);
        return creature;
    }
}