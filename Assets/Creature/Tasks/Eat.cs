using System.Collections.Generic;
using UnityEngine;

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
            var food = Creature.CarriedItem;
            Creature.CarriedItem = null;

            Creature.Hunger -= int.Parse(food.GetPropertyValue("Nutrition"));
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