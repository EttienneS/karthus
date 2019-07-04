public class Channel : TaskBase
{
    public Channel()
    {
    }

    public ManaColor ManaColor;
    public int AmountToChannel;

    public Channel(ManaColor color, int amount)
    {
        ManaColor = color;
        AmountToChannel = amount;
    }

    public override bool Done()
    {
        if (Faction.QueueComplete(SubTasks))
        {
            if (AmountToChannel <= 0)
            {
                return true;
            }
            else
            {
                Creature.Faction.ManaPool[ManaColor].Burn(1);
                AmountToChannel--;
                Creature.GainMana(ManaColor);

                var msg = $"{ManaColor}!!";
                AddSubTask(new Wait(1f, msg, true));
            }
        }

        return false;
    }
}
