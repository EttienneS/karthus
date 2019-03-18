using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class TaskBase 
{
    public int AssignedCreatureId;
    public string Originator;
    public string Context;
    public string Message;

    public bool Failed;

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

    public virtual bool Done()
    {
        return false;
    }

    public virtual void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}