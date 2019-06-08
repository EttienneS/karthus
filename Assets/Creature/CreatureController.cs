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

    public Creature SpawnPlayerAtLocation(CellData spawnLocation)
    {
        var Data = new CreatureData
        {
            Name = CreatureHelper.GetRandomName(),
            Coordinates = spawnLocation.Coordinates,
            Id = Creatures.Count + 1,
            Faction = FactionConstants.Player,
            GetBehaviourTask = Behaviours.PersonBehaviour
        };

        Data.ValueProperties[Prop.Hunger] = Random.Range(0, 15);
        Data.ValueProperties[Prop.Thirst] = Random.Range(35, 50);
        Data.ValueProperties[Prop.Energy] = Random.Range(80, 100);

        var creature = SpawnCreature(Data);
        creature.ShowText("Awee!!!", Random.Range(1f, 3f));
        return creature;
    }

    public void Start()
    {
        foreach (var creatureFile in Game.FileController.CreatureFiles)
        {
            var creature = CreatureData.Load(creatureFile.text);
            Beastiary.Add(creature.Name, creature);
        }
    }

    internal void DestroyCreature(Creature creature)
    {
        CreatureLookup.Remove(creature.Data);
        Destroy(creature.gameObject);
    }

    internal Creature GetCreatureForCreatureData(CreatureData creatureData)
    {
        return CreatureLookup[creatureData];
    }

    internal Creature SpawnCreature(CreatureData creatureData)
    {
        var creature = Instantiate(CreaturePrefab, transform, true);
        creature.Data = creatureData;
        creature.transform.position = creature.Data.Coordinates.ToMapVector();
        creature.Data.Id = Creatures.Count + 1;

        if (creature.Data.GetBehaviourTask == null)
        {
            creature.Data.GetBehaviourTask = Behaviours.ManaWraithBehaviour;
        }

        SetSprite(creature);
        IndexCreature(creature);

        return creature;
    }

    private static void SetSprite(Creature creature)
    {
        if (string.IsNullOrEmpty(creature.Data.Sprite))
        {
            creature.CreatureSprite = new ModularSprite(creature);
        }
        else
        {
            creature.CreatureSprite = new FixedCreatureSprite(creature.Data.Sprite, creature);
        }
    }

    private void IndexCreature(Creature creature)
    {
        Creatures.Add(creature);
        CreatureLookup.Add(creature.Data, creature);
        CreatureIdLookup.Add(creature.Data.Id, creature.Data);

        creature.name = $"{creature.Data.Name} ({creature.Data.Id})";
    }
}