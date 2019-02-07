using System;
using System.Collections.Generic;

[Serializable]
public class Eat : TaskBase
{
    public Eat()
    {
        SubTasks = new Queue<TaskBase>();
        SubTasks.Enqueue(new GetItemOfType("Food", true));
        SubTasks.Enqueue(new Wait(2f, "Eating"));
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
            Creature.CarriedItem = null;

            Creature.Hunger -= int.Parse(food.GetPropertyValue("Nutrition"));
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