using Newtonsoft.Json;
using System.Collections.Generic;

public abstract class TaskBase
{
    public int CreatureId;

    public string Originator;

    [JsonIgnore]
    public CreatureData Creature
    {
        get
        {
            return CreatureController.Instance.CreatureIdLookup[CreatureId];
        }
    }

    public Queue<TaskBase> SubTasks = new Queue<TaskBase>();

    public abstract bool Done();

    public abstract void Update();
}