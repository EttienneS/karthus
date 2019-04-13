public class Drink : TaskBase
{
    public Drink()
    {
    }

    public Drink(string itemCategory)
    {
        AddSubTask(new GetItem(itemCategory, true, GetItem.SearchBy.Category));
        AddSubTask(new Wait(3f, "Drinking"));

        Message = $"Drinking {itemCategory}";
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            if (Creature.CarriedItem == null)
            {
                throw new TaskFailedException("Nothing to drink");
            }

            var food = Creature.CarriedItem;
            Creature.DropItem();

            Creature.Thirst -= int.Parse(food.Properties["Quench"]);
            Game.ItemController.DestoyItem(food);

            return true;
        }

        return false;
    }
}