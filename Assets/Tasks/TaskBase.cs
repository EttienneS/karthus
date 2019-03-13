using Newtonsoft.Json;
using System.Collections.Generic;

public abstract class TaskBase
{
    public int AssignedCreatureId;
    public string Originator;
    public string Context;

    public Queue<TaskBase> SubTasks = new Queue<TaskBase>();

    [JsonIgnore]
    public CreatureData Creature
    {
        get
        {
            return CreatureController.Instance.CreatureIdLookup[AssignedCreatureId];
        }
    }

    public TaskBase AddSubTask(TaskBase subTask)
    {
        subTask.Context = Context;
        subTask.Originator = Originator;
        subTask.AssignedCreatureId = AssignedCreatureId;

        SubTasks.Enqueue(subTask);

        return subTask;
    }

    public abstract bool Done();

    public abstract void Update();
}