
using System.Collections.Generic;

public class Acrue : TaskBase
{
    public Acrue()
    {
    }

    public Dictionary<ManaColor, int> TargetManaLevel;

    public Acrue(Dictionary<ManaColor, int> targetManaLevel)
    {
        TargetManaLevel = targetManaLevel;
    }

    public override bool Done()
    {
        if (Faction.QueueComplete(SubTasks))
        {
            foreach (var targetLevel in TargetManaLevel)
            {
                if (!Creature.ManaPool.ContainsKey(targetLevel.Key))
                {
                    AddSubTask(Channel.GetChannelFrom(targetLevel.Key, targetLevel.Value, Creature.Faction.Core.Id));
                    return false;
                }
                else
                {
                    var currentLevel = Creature.ManaPool[targetLevel.Key].Total;
                    if (currentLevel < targetLevel.Value)
                    {
                        AddSubTask(Channel.GetChannelFrom(targetLevel.Key, targetLevel.Value - currentLevel, Creature.Faction.Core.Id));
                        return false;
                    }
                }
            }
            return true;
        }

        return false;
    }
}