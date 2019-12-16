﻿using System.Collections.Generic;

public class Eat : CreatureTask
{
    public override Dictionary<ManaColor, float> Cost => new Dictionary<ManaColor, float> { { FoodColor, 1 } };

    public Eat()
    {
    }

    public ManaColor FoodColor;

    public Eat(ManaColor foodColor) : this()
    {
        FoodColor = foodColor;
        var food = new Dictionary<ManaColor, float> { { FoodColor, 1 } };
        AddSubTask(new Acrue(food));
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            creature.ManaPool[FoodColor].Burn(1);
            creature.ValueProperties[Prop.Hunger] -= 50;
            return true;
        }

        return false;
    }
}