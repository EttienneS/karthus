using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreatureController : MonoBehaviour
{
    public CreatureRenderer CreaturePrefab;

    internal Dictionary<string, CreatureData> Beastiary = new Dictionary<string, CreatureData>();
    internal Dictionary<CreatureData, CreatureRenderer> CreatureLookup = new Dictionary<CreatureData, CreatureRenderer>();
    internal List<CreatureRenderer> Creatures = new List<CreatureRenderer>();

    public CreatureRenderer GetCreatureAtPoint(Vector2 point)
    {
        foreach (var creature in Creatures)
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
            CreatureLookup.Remove(creature.Data);
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

    internal CreatureRenderer SpawnCreature(CreatureData creatureData, Coordinates coordinates, Faction faction)
    {
        var creature = Instantiate(CreaturePrefab, transform, true);
        creature.Data = creatureData;
        creature.Data.Coordinates = coordinates;
        creature.transform.position = coordinates.ToMapVector();
        creature.Data.WorkTick = Random.Range(0, Game.TimeManager.WorkInterval);
        creature.Data.GetBehaviourTask = Behaviours.GetBehaviourFor(creature.Data.BehaviourName);

        creature.SpriteRenderer.material = Game.MaterialController.DefaultMaterial;
        creature.SpriteRenderer.sprite = Game.SpriteStore.GetCreatureSprite(creature.Data.Sprite);

        IndexCreature(creature);
        faction.AddCreature(creatureData);
        return creature;
    }


    private void IndexCreature(CreatureRenderer creature)
    {

        Creatures.Add(creature);
        CreatureLookup.Add(creature.Data, creature);
        IdService.EnrollEntity(creature.Data);

        creature.name = $"{creature.Data.Name} ({creature.Data.Id})";
    }
}