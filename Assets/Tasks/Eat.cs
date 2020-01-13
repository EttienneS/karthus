using System.Collections.Generic;
using System.Linq;

public class Eat : CreatureTask
{
    public bool DoneEating;

    public const string FoodCriteria = "Food";

    public Eat()
    {
        AddSubTask(new FindAndGetItem(FoodCriteria, 1));
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            if (!DoneEating)
            {
                AddSubTask(new Wait(2, "Eating...") { BusyEmote = "OMONONOMNOM" });
                DoneEating = true;
            }
            else
            {
                var food = creature.CarriedItems.FirstOrDefault(i => i.IsType(FoodCriteria));
                creature.Hunger -= food.ValueProperties["Nutrition"];
                creature.CarriedItemIds.Remove(food.Id);
                IdService.DestroyEntity(food);
                return true;
            }
        }
        return false;
    }

  
}