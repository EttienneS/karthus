using System.Linq;

public class Eat : CreatureTask
{
    public bool Eating;
    public bool Ate;

    public const string FoodCriteria = "Food";

    public Eat()
    {
    }

    public Eat(Item food)
    {
        AddSubTask(new Pickup(food, 1));
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {

            var food = creature.GetItemOfType(FoodCriteria);

            if (food == null)
            {
                AddSubTask(new FindAndGetItem(FoodCriteria, 1));
                return false;
            }

            if (!Eating)
            {
                AddSubTask(new Wait(2, "Eating...") { BusyEmote = "OMONONOMNOM" });
                Eating = true;
            }
            else if (!Ate)
            {
                var hunger = creature.DecreaseNeed(NeedNames.Hunger, food.ValueProperties["Nutrition"]);
                creature.CarriedItemIds.Remove(food.Id);
                Game.IdService.DestroyEntity(food);
                Ate = true;

                if (hunger > 20)
                {
                    AddSubTask(new Eat());
                }
            }
            else
            {
                Eating = false;
                Ate = false;
                return creature.GetNeed(NeedNames.Hunger).Current < 10;
            }
        }
        return false;
    }
}