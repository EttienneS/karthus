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
            FactionManager.Factions[Creature.Faction].ManaPool[ManaColor].Burn(1);
            Creature.ManaPool.GainMana(ManaColor, 1);
            AmountToChannel--;

            if (AmountToChannel <= 0)
            {
                return true;
            }
            else
            {
                var msg = $"{ManaColor}!!";
                AddSubTask(new Wait(1f, msg, true) { DoneEmote = msg });
                Creature.LinkedGameObject.PulseColor(ManaColor.GetActualColor(), 0.5f);
            }
        }

        return false;
    }
}