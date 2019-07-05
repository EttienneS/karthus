
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
                    AddSubTask(new Channel(targetLevel.Key, targetLevel.Value, Creature.Faction.Structure.GetGameId()));
                    return false;
                }
                else
                {
                    var currentLevel = Creature.ManaPool[targetLevel.Key].Total;
                    if (currentLevel < targetLevel.Value)
                    {
                        AddSubTask(new Channel(targetLevel.Key, targetLevel.Value - currentLevel, Creature.Faction.Structure.GetGameId()));
                        return false;
                    }
                }
            }
            return true;
        }

        return false;
    }
}