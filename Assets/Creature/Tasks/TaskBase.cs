using System;
using System.Collections.Generic;

[Serializable]
public abstract class TaskBase
{
    public CreatureData Creature { get; set; }
    public Queue<TaskBase> SubTasks { get; set; }

    public abstract bool Done();

    public abstract void Update();
}