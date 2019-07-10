using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreatureController : MonoBehaviour
{
    public Creature CreaturePrefab;

    internal Dictionary<string, CreatureData> Beastiary = new Dictionary<string, CreatureData>();
    internal Dictionary<int, CreatureData> CreatureIdLookup = new Dictionary<int, CreatureData>();
    internal Dictionary<CreatureData, Creature> CreatureLookup = new Dictionary<CreatureData, Creature>();
    internal List<Creature> Creatures = new List<Creature>();

    public Creature GetCreatureAtPoint(Vector2 point)
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

    internal void DestroyCreature(Creature creature)
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

    internal Creature GetCreatureForCreatureData(CreatureData creatureData)
    {
        return CreatureLookup[creatureData];
    }

    internal Creature SpawnCreature(CreatureData creatureData, Coordinates coordinates, Faction faction)
    {
        var creature = Instantiate(CreaturePrefab, transform, true);
        creature.Data = creatureData;
        creature.Data.Coordinates = coordinates;
        creature.transform.position = coordinates.ToMapVector();
        creature.Data.Id = IdService.UniqueId();

        creature.Data.GetBehaviourTask = Behaviours.GetBehaviourFor(creature.Data.BehaviourName);

        creature.Sprite.material = Game.MaterialController.DefaultMaterial;
        creature.Sprite.sprite = Game.SpriteStore.GetCreatureSprite(creature.Data.Sprite);

        IndexCreature(creature);
        faction.AddCreature(creatureData);
        return creature;
    }


    private void IndexCreature(Creature creature)
    {

        Creatures.Add(creature);
        CreatureLookup.Add(creature.Data, creature);
        CreatureIdLookup.Add(creature.Data.Id, creature.Data);

        creature.name = $"{creature.Data.Name} ({creature.Data.Id})";
    }
}