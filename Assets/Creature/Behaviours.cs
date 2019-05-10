using System.Linq;
using UnityEngine;

public static class Behaviours
{
    public delegate TaskBase GetBehaviourTaskDelegate(CreatureData creature);

    public static TaskBase MonsterBehaviour(CreatureData creature)
    {
        TaskBase task = null;
        if (Random.value > 0.5f)
        {
            var cell = Game.MapGrid.GetRandomCell();

            while (cell.Bound)
            {
                cell = Game.MapGrid.GetRandomCell();
            }

            task = new Move(cell.Coordinates);
        }
        else
        {
            task = new Sleep(creature.Coordinates, 10f);
        }

        return task;
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