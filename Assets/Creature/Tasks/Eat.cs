public class Eat : TaskBase
{
    public Eat()
    {
    }

    public Eat(string itemType)
    {
        AddSubTask(new GetItemOfType(itemType, true));
        AddSubTask(new Wait(2f, "Eating"));
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            if (Creature.CarriedItem == null)
            {
                throw new CancelTaskException("No food to eat");
            }

            var food = Creature.CarriedItem;
            Creature.DropItem();

            Creature.Hunger -= int.Parse(food.Properties["Nutrition"]);
            ItemController.Instance.DestoyItem(food);

            return true;
        }

        return false;
    }

    public override void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}