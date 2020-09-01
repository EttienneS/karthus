using Assets.Creature;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public delegate void TaskComplete();

public abstract class CreatureTask
{
    public string BusyEmote;
    public bool Destroyed;
    public string DoneEmote;

    [JsonIgnore]
    public ResumeDelegate OnResume;

    [JsonIgnore]
    public SuspendedDelegate OnSuspended;

    [JsonIgnore]
    public CreatureTask Parent;

    public Queue<CreatureTask> SubTasks = new Queue<CreatureTask>();
    private bool _suspended;

    public delegate void ResumeDelegate();

    public delegate void SuspendedDelegate();

    public bool AutoResume { get; set; }

    [JsonIgnore]
    public List<Badge> Badges { get; set; } = new List<Badge>();

    public Cost Cost { get; set; } = new Cost();
    public abstract string Message { get; }
    public string RequiredSkill { get; set; }

    public float RequiredSkillLevel { get; set; }

    [JsonIgnore]
    public Cost TotalCost
    {
        get
        {
            var total = Cost;

            foreach (var subTask in SubTasks)
            {
                total = Cost.AddCost(total, subTask.TotalCost);
            }

            return total;
        }
    }

    public void AddCellBadge(Cell cell, string badgeIcon)
    {
        Badges.Add(Game.Instance.VisualEffectController.AddBadge(cell, badgeIcon));
    }

    public void AddEntityBadge(IEntity badgedEntity, string badgeIcon)
    {
        Badges.Add(Game.Instance.VisualEffectController.AddBadge(badgedEntity, badgeIcon));
    }

    public CreatureTask AddSubTask(CreatureTask subTask)
    {
        subTask.Parent = this;

        SubTasks.Enqueue(subTask);

        return subTask;
    }

    public void Destroy()
    {
        foreach (var badge in Badges.Where(b => b != null))
        {
            badge.Destroy();
        }
        foreach (var task in SubTasks.Where(b => b != null))
        {
            task.Destroy();
        }
        Destroyed = true;
    }

    public abstract bool Done(CreatureData creature);

    public abstract void FinalizeTask();

    public bool IsSuspended()
    {
        return _suspended;
    }

    public void ShowBusyEmote(CreatureData creature)
    {
        if (!string.IsNullOrEmpty(BusyEmote))
        {
            creature.CreatureRenderer.ShowText(BusyEmote, 2f);
        }
    }

    public void ShowDoneEmote(CreatureData creature)
    {
        if (!string.IsNullOrEmpty(DoneEmote))
        {
            creature.CreatureRenderer.ShowText(DoneEmote, 2f);
        }
    }

    public bool SubTasksComplete(CreatureData creature)
    {
        if (SubTasks == null || SubTasks.Count == 0)
        {
            return true;
        }
        var current = SubTasks.Peek();
        if (current.Done(creature))
        {
            SubTasks.Dequeue();
            current.FinalizeTask();
        }
        return false;
    }

    internal void Resume()
    {
        _suspended = false;
        OnResume?.Invoke();
    }

    internal void Suspend(bool autoResume)
    {
        _suspended = true;
        AutoResume = autoResume;

        SubTasks.Clear();

        OnSuspended?.Invoke();
    }
}