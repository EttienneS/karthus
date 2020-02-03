using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Behaviours
{
    public static Dictionary<string, GetBehaviourTaskDelegate> BehaviourTypes = new Dictionary<string, GetBehaviourTaskDelegate>
    {
        { "Monster", Monster },
        { "Person", Person },
        { "Skeleton", Skeleton }
    };

    public static GetBehaviourTaskDelegate GetBehaviourFor(string type)
    {
        return BehaviourTypes[type];
    }

    public delegate CreatureTask GetBehaviourTaskDelegate(Creature creature);

    public const int WanderRange = 10;

    public static CreatureTask Monster(Creature creature)
    {
        CreatureTask task = null;

        var rand = Random.value;

        if (rand > 0.8f)
        {
            task = new Move(Game.Map.GetCircle(creature.Cell, WanderRange).GetRandomItem());
        }
        else
        {
            task = new Wait(Random.value * 2f, "Lingering..");
        }

        return task;
    }

    public static CreatureTask Skeleton(Creature creature)
    {
        CreatureTask task = null;

        var rand = Random.value;
        var targets = creature.Awareness.SelectMany(cell => cell.Creatures.Where(c => c.Faction != creature.Faction));

        creature.SetFixedAnimation(LPC.Spritesheet.Generator.Interfaces.Animation.Die, 4);

        if (targets.Any())
        {
            creature.ClearFixedAnimation();
            creature.Combatants.Add(targets.GetRandomItem());
        }
        else
        {
            task = new Wait(Random.Range(3f, 6f), "Lingering..", null);
        }

        return task;
    }

    public static CreatureTask Person(Creature creature)
    {
        CreatureTask task = null;

        var enemy = FindEnemy(creature);

        var wound = creature.GetWorstWound();
        if (enemy != null)
        {
            creature.Combatants.Add(enemy);
        }
        else if (wound != null)
        {
            task = new Heal();
        }
        else if (creature.ManaPool.Any(m => m.Value.Unbalanced()))
        {
            foreach (var mana in creature.ManaPool)
            {
                if (mana.Value.OverAttuned() || mana.Value.OverDesired())
                {
                    task = new Vent(mana.Key, mana.Value.Total - Mathf.Min(mana.Value.Desired, mana.Value.Attunement));
                    break;
                }
                else if (mana.Value.UnderDesired())
                {
                    task = new Attune(mana.Key, mana.Value.Desired);
                    break;
                }
            }
        }
        else if (creature.Cell.Creatures.Count > 1)
        {
            // split up
            task = new Move(Game.Map.GetPathableNeighbour(creature.Cell));
        }
        else if (creature.Hunger > 100)
        {
            task = new Eat();
        }
        else if (creature.Energy < 15)
        {
            var bed = creature.Self.Structures.FirstOrDefault(s => s.Properties.ContainsKey("RecoveryRate"));

            if (bed == null)
            {
                bed = Game.IdService.StructureIdLookup.Values
                                         .FirstOrDefault(s =>
                                                !s.InUseByAnyone
                                                && !s.IsBluePrint
                                                && s.Name == "Bed");
            }

            //if (bed != null)
            //{
            //    task = new Interact(bed.ActivatedInteractions[0], creature, bed.Id);
            //}
        }

        return task;
    }

    private static Creature FindEnemy(Creature creature)
    {
        return Game.IdService.CreatureIdLookup.Values.FirstOrDefault(c => c.FactionName != creature.FactionName
        && creature.Awareness.Contains(c.Cell));
    }
}