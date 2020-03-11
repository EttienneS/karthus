using Newtonsoft.Json;
using System.Collections.Generic;

public delegate void TaskComplete();

public class Cost
{
    public Dictionary<ManaColor, float> Mana = new Dictionary<ManaColor, float>();
    public Dictionary<string, int> Items = new Dictionary<string, int>();

    public static Cost AddCost(Cost cost1, Cost cost2)
    {
        var totalCost = new Cost()
        {
            Mana = ManaExtensions.AddPools(cost1.Mana, cost2.Mana),
            Items = cost1.Items
        };
        foreach (var kvp in cost2.Items)
        {
            if (!totalCost.Items.ContainsKey(kvp.Key))
            {
                totalCost.Items.Add(kvp.Key, 0);
            }

            totalCost.Items[kvp.Key] += kvp.Value;
        }

        return totalCost;
    }

    public override string ToString()
    {
        var costString = "Cost:\n";

        if (Mana.Keys.Count > 0)
        {
            costString += $"{Mana.GetString()}\n";
        }
        if (Items.Keys.Count > 0)
        {
            foreach (var item in Items)
            {
                costString += $"{item.Key}: x{item.Value}\n";
            }
        }
        return costString;
    }
}

public abstract class CreatureTask
{
    public string BusyEmote;
    public string DoneEmote;
    public string Message;

    public Cost Cost { get; set; } = new Cost();

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

    [JsonIgnore]
    public CreatureTask Parent;

    public Queue<CreatureTask> SubTasks = new Queue<CreatureTask>();

    public string RequiredSkill { get; set; }
    public float RequiredSkillLevel { get; set; }

    [JsonIgnore]
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

        SubTasks.Enqueue(subTask);

        return subTask;
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

    public abstract void Complete();
}