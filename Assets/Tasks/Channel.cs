public class Channel : TaskBase
{
    public Channel()
    {
    }

    public ManaColor ManaColor;
    public int AmountToChannel;
    public string SourceId;

    public Channel(ManaColor color, int amount, string sourceId)
    {
        ManaColor = color;
        AmountToChannel = amount;
        SourceId = sourceId;

        AddSubTask(new Move(Game.MapGrid.GetPathableNeighbour(IdService.GetLocation(sourceId))));
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
                IdService.GetMagicAttuned(SourceId)?.ManaPool.BurnMana(ManaColor, 1);
                AmountToChannel--;
                Creature.GainMana(ManaColor);

                var msg = $"{ManaColor}!!";
                AddSubTask(new Wait(1f, msg, true));
            }
        }

        return false;
    }
}
