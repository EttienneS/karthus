﻿using System.Collections.Generic;

public class DropHeldItem : ITask
{
    public Creature Creature { get; set; }

    public Queue<ITask> SubTasks { get; set; }
    public string TaskId { get; set; }

    public bool Done()
    {
        return Creature.CarriedItem == null;
    }

    public void Update()
    {
        Creature.CarriedItem.SpriteRenderer.sortingLayerName = "Item";
        Creature.CarriedItem = null;
    }
}
