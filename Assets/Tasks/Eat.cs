public class Eat : TaskBase
{
    public Eat()
    {
    }

    public Eat(string itemCategory)
    {
        AddSubTask(new GetItem(itemCategory, true, GetItem.SearchBy.Category));
        AddSubTask(new Wait(2f, "Eating"));

        Message = $"Eating {itemCategory}";
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            if (Creature.CarriedItem == null)
            {
                throw new TaskFailedException("No food to eat");
            }

            var food = Creature.CarriedItem;
            Creature.DropItem();

            Creature.Hunger -= int.Parse(food.Properties["Nutrition"]);
            ItemController.Instance.DestoyItem(food);

            return true;
        }

        return false;
    }

    
}