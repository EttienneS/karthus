using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StockpileItem : TaskBase
{
    public StockpileItem(string itemType, Stockpile stockpile)
    {
        SubTasks = new Queue<TaskBase>();
        Stockpile = stockpile;
        SubTasks.Enqueue(new MoveItemToCell(itemType, stockpile.Coordinates, false));
    }

    
    public Stockpile Stockpile { get; set; }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            if (Creature.CarriedItem == null)
            {
                return true;
            }

            Stockpile.AddItem(Creature.CarriedItem);
            Creature.CarriedItem = null;
            return true;
        }

        return false;
    }

    public override void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}