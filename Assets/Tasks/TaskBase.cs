using Newtonsoft.Json;
using System.Collections.Generic;

public class TaskBase
{
    public IEntity AssignedCreature;
    public IEntity Originator;
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
            if (IdService.CreatureLookup.ContainsKey(AssignedCreature))
            {
                return IdService.CreatureLookup[AssignedCreature];
            }
            return null;
        }
    }

    public TaskBase AddSubTask(TaskBase subTask)
    {
        subTask.Context = Context;
        subTask.Originator = Originator;
        subTask.AssignedCreature = AssignedCreature;

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
            Creature.CreatureRenderer.ShowText(BusyEmote, 1f);
        }
    }

    public void ShowDoneEmote()
    {
        if (!string.IsNullOrEmpty(DoneEmote))
        {
            Creature.CreatureRenderer.ShowText(DoneEmote, 0.8f);
        }
    }
}