using System.Linq;

public static class Behaviours
{
    public delegate TaskBase GetBehaviourTaskDelegate(CreatureData creature);

    public static TaskBase MonsterBehaviour(CreatureData creature)
    {
        return new Sleep(creature.Coordinates, 2f);
    }

    public static TaskBase PersonBehaviour(CreatureData creature)
    {
        TaskBase task = null;

        if (creature.ValueProperties[Prop.Hunger] > 50)
        {
            task = new Eat("Food");
        }
        else if (creature.ValueProperties[Prop.Thirst] > 50)
        {
            task = new Drink("Drink");
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