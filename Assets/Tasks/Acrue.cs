using System.Collections.Generic;

public class Acrue : EntityTask
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
        if (SubTasksComplete())
        {
            foreach (var targetLevel in TargetManaLevel)
            {
                if (!AssignedEntity.ManaPool.ContainsKey(targetLevel.Key))
                {
                    AddSubTask(Channel.GetChannelFrom(targetLevel.Key, targetLevel.Value, AssignedEntity.GetFaction().Core));
                    return false;
                }
                else
                {
                    var currentLevel = AssignedEntity.ManaPool[targetLevel.Key].Total;
                    if (currentLevel < targetLevel.Value)
                    {
                        AddSubTask(Channel.GetChannelFrom(targetLevel.Key, targetLevel.Value - currentLevel, AssignedEntity.GetFaction().Core));
                        return false;
                    }
                }
            }
            return true;
        }

        return false;
    }
}