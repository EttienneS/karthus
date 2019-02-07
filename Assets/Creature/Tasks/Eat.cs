﻿using System.Collections.Generic;

public class Eat : ITask
{
    public Creature Creature { get; set; }

    public Eat()
    {
        SubTasks = new Queue<ITask>();
        SubTasks.Enqueue(new GetItemOfType("Food", true));
        SubTasks.Enqueue(new Wait(2f, "Eating"));
    }

    public Queue<ITask> SubTasks { get; set; }
    public string TaskId { get; set; }

    public bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            if (Creature.Data.CarriedItem == null)
            {
                throw new CancelTaskException("No food to eat");
            }

            var food = Creature.Data.CarriedItem;
            Creature.Data.CarriedItem = null;

            Creature.Data.Hunger -= int.Parse(food.GetPropertyValue("Nutrition"));
            ItemController.Instance.DestoyItem(food);

            return true;
        }

        return false;
    }

    public void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}