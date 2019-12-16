using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Behaviours
{
    public static Dictionary<string, GetBehaviourTaskDelegate> BehaviourTypes = new Dictionary<string, GetBehaviourTaskDelegate>
    {
        { "Monster", Monster },
        { "Person", Person }
    };

    public static GetBehaviourTaskDelegate GetBehaviourFor(string type)
    {
        return BehaviourTypes[type];
    }

    public delegate CreatureTask GetBehaviourTaskDelegate(Creature creature);

    public const int WraithRange = 10;

    public static CreatureTask Monster(Creature creature)
    {
        CreatureTask task = null;

        var rand = Random.value;

        if (rand > 0.999f)
        {
            if (Game.FactionController.PlayerFaction.Creatures.Count > 0)
            {
                creature.Combatants.Add(Game.FactionController.PlayerFaction.Creatures.GetRandomItem());
            }
        }
        else if (rand > 0.8f)
        {
            task = new Move(Game.Map.GetRectangle(creature.Cell.X - (WraithRange / 2),
                creature.Cell.Y - (WraithRange / 2), WraithRange, WraithRange).GetRandomItem());
        }
        else
        {
            task = new Wait(Random.value * 2f, "Lingering..");
        }
        if (Game.FactionController.PlayerFaction.Creatures.Count > 0)
        {
        }
        else
        {
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
            var factionMana = creature.Faction.GetStoredMana();
            foreach (var mana in creature.ManaPool)
            {
                if (mana.Value.OverAttuned() || mana.Value.OverDesired())
                {
                    var min = Mathf.Min(mana.Value.Desired, mana.Value.Attunement);
                    task = Channel.GetChannelTo(mana.Key, mana.Value.Total - min, creature.Faction.GetClosestBattery(creature));
                    break;
                }
                else if (mana.Value.UnderDesired())
                {
                    var want = mana.Value.Desired - mana.Value.Total;
                    if (factionMana.HasMana(mana.Key, want))
                    {
                        task = Channel.GetChannelFrom(mana.Key, want, creature.GetSource());
                    }
                    break;
                }
            }
        }
        else if (creature.Cell.Creatures.Count > 1)
        {
            // split up
            task = new Move(Game.Map.GetPathableNeighbour(creature.Cell));
        }
        else if (creature.ValueProperties[Prop.Hunger] > 50)
        {
            task = new Eat(ManaColor.Green);
        }
        else if (creature.ValueProperties[Prop.Energy] < 15)
        {
            var bed = creature.Self.Structures.FirstOrDefault(s => s.Properties.ContainsKey("RecoveryRate"));

            if (bed == null)
            {
                bed = IdService.StructureIdLookup.Values
                                         .FirstOrDefault(s =>
                                                !s.InUseByAnyone
                                                && !s.IsBluePrint
                                                && s.Name == "Bed");
            }

            if (bed != null)
            {
                task = new Interact(bed.ActivatedInteractions[0], creature, bed.Id);
            }
        }

        return task;
    }

    private static Creature FindEnemy(Creature creature)
    {
        return IdService.CreatureIdLookup.Values.FirstOrDefault(c => c.FactionName != creature.FactionName
        && creature.Awareness.Contains(c.Cell));
    }
}