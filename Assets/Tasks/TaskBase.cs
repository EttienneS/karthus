using Newtonsoft.Json;
using System.Collections.Generic;

public delegate void TaskComplete();

public abstract class EntityTask
{
    public IEntity AssignedEntity;
    public string BusyEmote;
    public string Context;
    public string DoneEmote;
    public bool Failed;
    public string Message;
    public IEntity Originator;
    public EntityTask Parent;

    public Queue<EntityTask> SubTasks = new Queue<EntityTask>();

    private CreatureData _creatureData;

    private bool? _isCreature;

    private bool? _isStructure;

    private Structure _structure;

    [JsonIgnore]
    public TaskComplete CompleteEvent { get; set; }

    [JsonIgnore]
    public CreatureData CreatureData
    {
        get
        {
            if (_creatureData == null && IsCreature)
            {
                _creatureData = AssignedEntity as CreatureData;
            }
            return _creatureData;
        }
    }

    [JsonIgnore]
    public bool IsCreature
    {
        get
        {
            if (_isCreature == null)
            {
                _isCreature = AssignedEntity is CreatureData;
            }
            return _isCreature.Value;
        }
    }

    [JsonIgnore]
    public bool IsStructure
    {
        get
        {
            if (_isStructure == null)
            {
                _isStructure = AssignedEntity is Structure;
            }
            return _isStructure.Value;
        }
    }

    [JsonIgnore]
    public Structure Structure
    {
        get
        {
            if (_structure == null && IsStructure)
            {
                _structure = AssignedEntity as Structure;
            }
            return _structure;
        }
    }

    public EntityTask AddSubTask(EntityTask subTask)
    {
        subTask.Parent = this;
        subTask.Context = Context;
        subTask.Originator = Originator;
        subTask.AssignedEntity = AssignedEntity;

        SubTasks.Enqueue(subTask);

        return subTask;
    }

    public void CancelTask()
    {
        if (AssignedEntity != null)
        {
            AssignedEntity.GetFaction().CancelTask(this);
        }
    }

    public abstract bool Done();

    public void ShowBusyEmote()
    {
        if (!string.IsNullOrEmpty(BusyEmote))
        {
            CreatureData?.CreatureRenderer.ShowText(BusyEmote, 1f);
        }
    }

    public void ShowDoneEmote()
    {
        if (!string.IsNullOrEmpty(DoneEmote))
        {
            CreatureData?.CreatureRenderer.ShowText(DoneEmote, 0.8f);
        }
    }

    public bool SubTasksComplete()
    {
        if (SubTasks == null || SubTasks.Count == 0)
        {
            return true;
        }
        var current = SubTasks.Peek();
        if (current.Done())
        {
            SubTasks.Dequeue();
        }
        return false;
    }
}