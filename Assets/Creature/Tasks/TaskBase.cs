using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public abstract class TaskBase
{
    
    public Queue<TaskBase> SubTasks { get; set; }

    
    public CreatureData Creature { get; set; }

    
    public string TaskId { get; set; }

    public abstract  bool Done();

    public abstract void Update();
}