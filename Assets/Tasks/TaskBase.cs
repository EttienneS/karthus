using System.Collections.Generic;

public delegate void TaskComplete();

public abstract class CreatureTask
{
    public string BusyEmote;
    public string Context;
    public string DoneEmote;
    public string Message;
    public CreatureTask Parent;

    public Queue<CreatureTask> SubTasks = new Queue<CreatureTask>();

    public string RequiredSkill { get; set; }
    public float RequiredSkillLevel { get; set; }

    public List<Badge> Badges { get; set; } = new List<Badge>();

    public void AddCellBadge(Cell cell, string badgeIcon)
    {
        Badges.Add(Game.VisualEffectController.AddBadge(cell, badgeIcon));
    }

    public void AddEntityBadge(IEntity badgedEntity, string badgeIcon)
    {
        Badges.Add(Game.VisualEffectController.AddBadge(badgedEntity, badgeIcon));
    }

    public CreatureTask AddSubTask(CreatureTask subTask)
    {
        subTask.Parent = this;
        subTask.Context = Context;

        SubTasks.Enqueue(subTask);

        return subTask;
    }

    public abstract bool Done(CreatureData creature);

    public void ShowBusyEmote(CreatureData creature)
    {
        if (!string.IsNullOrEmpty(BusyEmote))
        {
            creature.CreatureRenderer.ShowText(BusyEmote, 1f);
        }
    }

    public void ShowDoneEmote(CreatureData creature)
    {
        if (!string.IsNullOrEmpty(DoneEmote))
        {
            creature.CreatureRenderer.ShowText(DoneEmote, 0.8f);
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
        }
        return false;
    }

    public bool Destroyed;

    public void Destroy()
    {
        foreach (var badge in Badges)
        {
            badge.Destroy();
        }
        foreach (var task in SubTasks)
        {
            task.Destroy();
        }
        Destroyed = true;
    }
}