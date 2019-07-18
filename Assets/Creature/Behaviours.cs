using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public static class Behaviours
{
    public static Dictionary<string, GetBehaviourTaskDelegate> BehaviourTypes = new Dictionary<string, GetBehaviourTaskDelegate>
    {
        { "AbyssWraith", AbyssWraith },
        { "Person", Person }
    };

    public static GetBehaviourTaskDelegate GetBehaviourFor(string type)
    {
        return BehaviourTypes[type];
    }

    public delegate TaskBase GetBehaviourTaskDelegate(CreatureData creature);

    public const int WraithRange = 10;

    public static TaskBase AbyssWraith(CreatureData creature)
    {
        TaskBase task = null;
        if (Random.value > 0.8f)
        {
            task = new Move(Game.MapGrid.GetRectangle(creature.Coordinates.X - (WraithRange / 2),
                creature.Coordinates.Y - (WraithRange / 2), WraithRange, WraithRange).GetRandomItem().Coordinates);
        }
        else
        {
            task = new Wait(Random.value * 5f, "Lingering...");
        }

        return task;
    }

    public static TaskBase Person(CreatureData creature)
    {
        TaskBase task = null;

        const int threshold = 5;
        if (creature.ManaPool.Any(m => m.Value.Total > threshold))
        {
            foreach (var mana in creature.ManaPool)
            {
                if (mana.Value.Total > threshold)
                {
                    task = Channel.GetChannelTo(mana.Key, mana.Value.Total, creature.Faction.Structure.GetGameId());
                    break;
                }
            }
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
                bed = Game.StructureController.StructureLookup.Keys
                                         .FirstOrDefault(s =>
                                                !s.InUseByAnyone
                                                && s.Properties.ContainsKey("RecoveryRate"));
            }

            if (bed == null)
            {
                task = new Sleep(creature.Coordinates, 0.25f);
            }
            else
            {
                task = new Sleep(bed);
            }
        }

        return task;
    }
}