using System.Collections.Generic;
public class BurnManaTask : CreatureTask
{
    public BurnManaTask()
    {
        RequiredSkill = "Arcana";
        RequiredSkillLevel = 1;
    }

    public BurnManaTask(ManaColor color, float amount) : this()
    {
        Color = color;
        Amount = amount;
    }

    public float Amount { get; set; }
    public ManaColor Color { get; set; }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            creature.ManaPool.BurnMana(Color, Amount);
            return true;
        }

        return false;
    }
}
