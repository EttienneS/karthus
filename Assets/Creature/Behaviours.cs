using Needs;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public static class Behaviours
{
    public static Dictionary<string, GetBehaviourTaskDelegate> BehaviourTypes = new Dictionary<string, GetBehaviourTaskDelegate>
    {
        { "Person", Person },
        { "Grazer", Grazer }
    };

    public delegate CreatureTask GetBehaviourTaskDelegate(Creature creature);

    public static GetBehaviourTaskDelegate GetBehaviourFor(string type)
    {
        return BehaviourTypes[type];
    }

    public static CreatureTask Grazer(Creature creature)
    {
        var creatures = creature.Awareness.SelectMany(c => c.Creatures);

        var enemies = creatures.Where(c => c.FactionName != creature.FactionName);
        var herd = creatures.Where(c => c.FactionName == creature.FactionName);

        if (enemies.Any())
        {
            var target = Game.Instance.Map.GetCellAttRadian(enemies.GetRandomItem().Cell, 10, Random.Range(1, 360));
            return new Move(target);
        }
        else if (herd.Any())
        {
            return new Move(Game.Instance.Map.GetCircle(herd.GetRandomItem().Cell, 3).GetRandomItem());
        }

        return null;
    }

    public static CreatureTask Person(Creature creature)
    {
        var wound = creature.GetWorstWound();
        if (wound != null)
        {
            return new Heal();
        }
        else if (creature.Cell.Creatures.Count > 1)
        {
            // split up
            return new Move(Game.Instance.Map.TryGetPathableNeighbour(creature.Cell));
        }

        return null;
    }
    internal static List<NeedBase> GetNeedsFor(string behaviourName)
    {
        var needs = new List<NeedBase>();
        switch (behaviourName.ToLower())
        {
            case "person":
                needs = new List<NeedBase>
                {
                    new Hunger(),
                    new Energy(),
                    new Comfort(),
                    new Hygiene(),
                    new Needs.Social()
                };
                break;

            default:
                needs = new List<NeedBase>
                {
                    new Hunger(),
                    new Energy(),
                };
                break;
        }
        return needs;
    }

    
}