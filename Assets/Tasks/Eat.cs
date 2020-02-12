﻿using System.Linq;

public class Eat : CreatureTask
{
    public bool Eating;
    public bool Ate;

    public const string FoodCriteria = "Food";

    public Eat()
    {
        AddSubTask(new FindAndGetItem(FoodCriteria, 1));
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            if (!Eating)
            {
                AddSubTask(new Wait(2, "Eating...") { BusyEmote = "OMONONOMNOM" });
                Eating = true;
            }
            else if (!Ate)
            {
                var food = creature.GetItemOfType(FoodCriteria);
                creature.Hunger -= food.ValueProperties["Nutrition"];
                creature.CarriedItemIds.Remove(food.Id);
                Game.IdService.DestroyEntity(food);
                Ate = true;

                if (creature.Hunger > 10)
                {
                    AddSubTask(new Eat());
                }
            }
            else
            {
                return creature.Hunger < 10;
            }
        }
        return false;
    }
}