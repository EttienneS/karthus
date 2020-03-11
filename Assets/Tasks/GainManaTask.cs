public class GainManaTask : CreatureTask
{
    public ManaColor Color { get; set; }
    public float Amount { get; set; }

    public GainManaTask()
    {
        RequiredSkill = SkillConstants.Arcana;
        RequiredSkillLevel = 1;
    }

    public GainManaTask(ManaColor color, float amount) : this()
    {
        Color = color;
        Amount = amount;
    }

    public override void Complete()
    {
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            creature.ManaPool.GainMana(Color, Amount);
            return true;
        }

        return false;
    }
}