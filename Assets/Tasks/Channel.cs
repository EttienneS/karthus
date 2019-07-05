public class Channel : TaskBase
{
    public Channel()
    {
    }

    public ManaColor ManaColor;
    public int AmountToChannel;
    public string Source;

    public Channel(ManaColor color, int amount, string sourceId)
    {
        ManaColor = color;
        AmountToChannel = amount;
        Source = sourceId;

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
                switch (IdService.GetObjectTypeForId(Source))
                {
                    case IdService.ObjectType.Creature:
                        IdService.GetCreatureFromId(Source).ManaPool[ManaColor].Burn(1);
                        break;
                    case IdService.ObjectType.Structure:
                        IdService.GetStructureFromId(Source).ManaPool[ManaColor].Burn(1);
                        break;
                }

                AmountToChannel--;
                Creature.GainMana(ManaColor);

                var msg = $"{ManaColor}!!";
                AddSubTask(new Wait(1f, msg, true));
            }
        }

        return false;
    }
}
