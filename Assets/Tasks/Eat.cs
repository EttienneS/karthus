using System.Collections.Generic;

public class Eat : Task
{
    public Eat()
    {
    }

    public ManaColor FoodColor;

    public Eat(ManaColor foodColor)
    {
        FoodColor = foodColor;
        var food = new Dictionary<ManaColor, int> { { FoodColor, 1 } };
        AddSubTask(new Acrue(food));
    }

    public override bool Done()
    {
        if (Creature.TaskQueueComplete(SubTasks))
        {
            Creature.ManaPool[FoodColor].Burn(1);
            Creature.ValueProperties[Prop.Hunger] -= 50;
            return true;
        }

        return false;
    }
}