using Newtonsoft.Json;
using System.Collections.Generic;

public class TaskBase
{
    public int AssignedCreatureId;
    public string Originator;
    public string Context;
    public string Message;

    public string BusyEmote;
    public string DoneEmote;

    public bool Failed;

    public Queue<TaskBase> SubTasks = new Queue<TaskBase>();

    [JsonIgnore]
    public CreatureData Creature
    {
        get
        {
            if (Game.CreatureController.CreatureIdLookup.ContainsKey(AssignedCreatureId))
            {
                return Game.CreatureController.CreatureIdLookup[AssignedCreatureId];
            }
            return null;
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

    public void ShowBusyEmote()
    {
        if (!string.IsNullOrEmpty(BusyEmote))
        {
            Creature.LinkedGameObject.ShowText(BusyEmote, 1f);
        }
    }

    public void ShowDoneEmote()
    {
        if (!string.IsNullOrEmpty(DoneEmote))
        {
            Creature.LinkedGameObject.ShowText(DoneEmote, 0.8f);
        }
    }
}