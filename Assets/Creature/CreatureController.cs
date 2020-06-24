using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreatureController : MonoBehaviour
{
    //public CreatureRenderer CreaturePrefab;
    public SpriteRenderer HightlightPrefab;
    public List<CreatureRenderer> AllPrefabs;

    private Dictionary<string, Creature> _beastiary;

    internal Dictionary<string, Creature> Beastiary
    {
        get
        {
            if (_beastiary == null)
            {
                _beastiary = new Dictionary<string, Creature>();
                foreach (var creatureFile in Game.Instance.FileController.CreatureFiles)
                {
                    try
                    {
                        var creature = creatureFile.text.LoadJson<Creature>();
                        _beastiary.Add(creature.Name, creature);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Unable to load creature {creatureFile}: {ex.Message}");
                    }
                }
            }
            return _beastiary;
        }
    }

    public CreatureRenderer GetCreatureAtPoint(Vector2 point)
    {
        foreach (var creature in Game.Instance.IdService.CreatureLookup.Values)
        {
            var rect = new Rect(creature.CreatureRenderer.transform.position.x - 0.5f, creature.CreatureRenderer.transform.position.y - 0.5f, 1f, 1f);
            if (rect.Contains(point))
            {
                return creature.CreatureRenderer;
            }
        }

        return null;
    }

    internal void DestroyCreature(CreatureRenderer creature)
    {
        if (creature != null)
        {
            Debug.Log($"Destroying: {creature.Data.Name}");
            if (creature.Data.Task != null)
                creature.Data.AbandonTask();

            Game.Instance.FactionController.Factions[creature.Data.FactionName].Creatures.Remove(creature.Data);
            Game.Instance.IdService.RemoveEntity(creature.Data);
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
        var prefab = AllPrefabs.First(c => c.name.Equals(creatureData.Model, StringComparison.OrdinalIgnoreCase));
        var creature = Instantiate(prefab, transform);

        creature.Data = creatureData;
        creature.Data.CreatureRenderer = creature;

        Game.Instance.IdService.EnrollEntity(creature.Data);
        creature.name = $"{creature.Data.Name} ({creature.Data.Id})";

        if (creatureData.BehaviourName == "Person")
        {
            creature.Data.Name = NameHelper.GetRandomName();
        }
        else
        {
            creature.Data.Name = creatureData.BehaviourName;
        }

        creature.Data.X = cell.Vector.x + Random.Range(-0.25f, 0.25f);
        creature.Data.Z = cell.Vector.z + Random.Range(-0.25f, 0.25f);
        creature.UpdatePosition();

        creature.Data.InternalTick = Random.Range(0, Game.Instance.TimeManager.CreatureTick);

        creature.Data.GetBehaviourTask = Behaviours.GetBehaviourFor(creature.Data.BehaviourName);

        creature.Data.Needs = Behaviours.GetNeedsFor(creature.Data.BehaviourName);

        faction.AddCreature(creatureData);

        return creature;
    }
}