using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public abstract class TaskBase
{

    public int CreatureId;

    [JsonIgnore]
    public CreatureData Creature
    {
        get
        {
            return CreatureController.Instance.CreatureIdLookup[CreatureId];
        }
    }

    public Queue<TaskBase> SubTasks { get; set; }

    public abstract bool Done();

    public abstract void Update();
}