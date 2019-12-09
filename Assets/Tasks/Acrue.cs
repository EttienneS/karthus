using System.Collections.Generic;

public class Acrue : CreatureTask
{
    public Acrue()
    {
        RequiredSkill = "Arcana";
        RequiredSkillLevel = 1;
    }

    public Dictionary<ManaColor, int> TargetManaLevel;

    public Acrue(Dictionary<ManaColor, int> targetManaLevel)
    {
        TargetManaLevel = targetManaLevel;
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            foreach (var targetLevel in TargetManaLevel)
            {
                if (!creature.ManaPool.ContainsKey(targetLevel.Key))
                {
                    AddSubTask(Channel.GetChannelFrom(targetLevel.Key, targetLevel.Value, creature.GetClosestBattery()));
                    return false;
                }
                else
                {
                    var currentLevel = creature.ManaPool[targetLevel.Key].Total;
                    if (currentLevel < targetLevel.Value)
                    {
                        AddSubTask(Channel.GetChannelFrom(targetLevel.Key, targetLevel.Value - currentLevel, creature.GetClosestBattery()));
                        return false;
                    }
                }
            }
            return true;
        }

        return false;
    }
}