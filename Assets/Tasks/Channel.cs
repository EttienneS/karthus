public class Channel : TaskBase
{
    public Channel()
    {
    }

    public ManaColor ManaColor;
    public int AmountToChannel;
    public string Source;
    public string Target;

    public static Channel GetChannelTo(ManaColor color, int amount, string target)
    {
        var task = new Channel
        {
            ManaColor = color,
            AmountToChannel = amount,
            Target = target
        };

        task.AddSubTask(new Move(Game.MapGrid.GetPathableNeighbour(IdService.GetLocation(target))));

        return task;
    }

    public static Channel GetChannelFrom(ManaColor color, int amount, string source)
    {
        var task = new Channel
        {
            ManaColor = color,
            AmountToChannel = amount,
            Source = source
        };

        task.AddSubTask(new Move(Game.MapGrid.GetPathableNeighbour(IdService.GetLocation(source))));

        return task;
    }

    public override bool Done()
    {
        if (Source == null)
        {
            Source = Creature.GetGameId();
        }
        else if (Target == null)
        {
            Target = Creature.GetGameId();
        }

        if (Faction.QueueComplete(SubTasks))
        {
            if (AmountToChannel <= 0)
            {
                return true;
            }
            else
            {
                IdService.GetMagicAttuned(Source)?.ManaPool.BurnMana(ManaColor, 1);
                IdService.GetMagicAttuned(Target)?.ManaPool.GainMana(ManaColor, 1);

                Game.LeyLineController.MakeChannellingLine(IdService.GetLocation(Source).ToTopOfMapVector(),
                    IdService.GetLocation(Target).ToTopOfMapVector(), 5, GameConstants.ChannelDuration, ManaColor);
                Creature.LinkedGameObject.DoChannel(ManaColor, GameConstants.ChannelDuration);
                AmountToChannel--;
                AddSubTask(new Wait(2f, $"{ManaColor}!!", true));
            }
        }

        return false;
    }
}