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
    public CreatureTask Parent;

    public Queue<CreatureTask> SubTasks = new Queue<CreatureTask>();
    public bool AutoRetry { get; set; }
    [JsonIgnore]
    public List<Badge> Badges { get; set; } = new List<Badge>();

    public Cost Cost { get; set; } = new Cost();
    public abstract string Message { get; }
    public string RequiredSkill { get; set; }

    public float RequiredSkillLevel { get; set; }

    public bool Suspended { get; set; }

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

    public abstract void Complete();

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

    public abstract bool Done(Creature creature);

    public void ShowBusyEmote(Creature creature)
    {
        if (!string.IsNullOrEmpty(BusyEmote))
        {
            creature.CreatureRenderer.ShowText(BusyEmote, 2f);
        }
    }

    public void ShowDoneEmote(Creature creature)
    {
        if (!string.IsNullOrEmpty(DoneEmote))
        {
            creature.CreatureRenderer.ShowText(DoneEmote, 2f);
        }
    }

    public bool SubTasksComplete(Creature creature)
    {
        if (SubTasks == null || SubTasks.Count == 0)
        {
            return true;
        }
        var current = SubTasks.Peek();
        if (current.Done(creature))
        {
            SubTasks.Dequeue();
            current.Complete();
        }
        return false;
    }

    internal void ToggleSuspended(bool autoRetry)
    {
        Suspended = !Suspended;
        AutoRetry = true;
    }
}