using System.Collections.Generic;
using System.Linq;

public class Eat : CreatureTask
{
    public string TargetEntityId;
    public string FoodItemId;

    public Eat()
    {
        AddSubTask(new FindAndGetItem("Food", 1));
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            if (string.IsNullOrEmpty(TargetEntityId))
            {
                var targetEntity = FindFood(creature);
                TargetEntityId = targetEntity.Id;
                AddSubTask(new Move(targetEntity.Cell));
            }
            else if (string.IsNullOrEmpty(FoodItemId))
            {
                var foodEntity = TargetEntityId.GetEntity();
                Item foodItem = null;
                if (foodEntity is Item item)
                {
                    foodItem = item;
                }
                else if (foodEntity is Container container)
                {
                    foodItem = container.GetItem(1);
                }
                creature.PickUpItem(foodItem, 1);
                FoodItemId = foodItem.Id;
                AddSubTask(new Wait(2, "Eating...") { BusyEmote = "OMONONOMNOM" });
            }
            else
            {
                var food = FoodItemId.GetItem();
                creature.Hunger -= food.ValueProperties["Nutrition"];
                IdService.DestroyEntity(food);

                return true;
            }
        }
        return false;
    }

    private static IEntity FindFood(Creature creature)
    {
        var eats = "Food";

        var foodEntities = new List<IEntity>();
        foodEntities.AddRange(creature.Faction.StorageZones.SelectMany(zone => zone.Containers.Where(c => c.HasItemOfType(eats))));
        foodEntities.AddRange(Game.Map.GetCircle(creature.Cell, 10).SelectMany(c => c.Items.Where(i => i.IsType(eats))));

        var bestNutrition = 0f;
        IEntity targetEntity = null;
        var bestDistance = float.MaxValue;

        foreach (var foodEntity in foodEntities)
        {
            var best = false;
            var cell = foodEntity.Cell;
            var distance = Pathfinder.Distance(creature.Cell, targetEntity.Cell, creature.Mobility);

            var nutrition = 0f;

            if (foodEntity is Item item)
            {
                nutrition = item.ValueProperties["Nutrition"];
            }
            else if (foodEntity is Container container)
            {
                nutrition = container.GetContainedItemTemplate().ValueProperties["Nutrition"];
            }

            if (targetEntity == null)
            {
                best = true;
            }
            else
            {
                best = (nutrition / distance) > (bestNutrition / bestDistance);
            }

            if (best)
            {
                targetEntity = foodEntity;
                bestNutrition = nutrition;
                bestDistance = distance;
            }
        }

        return targetEntity;
    }
}