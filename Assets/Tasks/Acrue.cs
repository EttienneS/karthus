using System.Collections.Generic;

public class Acrue : CreatureTask
{
    public Acrue()
    {
    }

    public Dictionary<ManaColor, int> TargetManaLevel;

    public Acrue(Dictionary<ManaColor, int> targetManaLevel)
    {
        TargetManaLevel = targetManaLevel;
    }

    public override bool Done(CreatureData creature)
    {
        if (SubTasksComplete(creature))
        {
            foreach (var targetLevel in TargetManaLevel)
            {
                if (!creature.ManaPool.ContainsKey(targetLevel.Key))
                {
                    AddSubTask(Channel.GetChannelFrom(targetLevel.Key, targetLevel.Value, creature.GetFaction().Core));
                    return false;
                }
                else
                {
                    var currentLevel = creature.ManaPool[targetLevel.Key].Total;
                    if (currentLevel < targetLevel.Value)
                    {
                        AddSubTask(Channel.GetChannelFrom(targetLevel.Key, targetLevel.Value - currentLevel, creature.GetFaction().Core));
                        return false;
                    }
                }
            }
            return true;
        }

        return false;
    }
}