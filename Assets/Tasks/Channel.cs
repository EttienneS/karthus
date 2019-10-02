public class Channel : EntityTask
{
    public Channel()
    {
    }

    public ManaColor ManaColor;
    public int AmountToChannel;
    public IEntity Source;
    public IEntity Target;

    public static Channel GetChannelTo(ManaColor color, int amount, IEntity target)
    {
        var task = new Channel
        {
            ManaColor = color,
            AmountToChannel = amount,
            Target = target
        };

        task.AddSubTask(new Move(Game.Map.GetPathableNeighbour(target.Cell)));

        return task;
    }

    public static Channel GetChannelFrom(ManaColor color, int amount, IEntity source)
    {
        var task = new Channel
        {
            ManaColor = color,
            AmountToChannel = amount,
            Source = source
        };

        task.AddSubTask(new Move(Game.Map.GetPathableNeighbour(source.Cell)));

        return task;
    }

    public override bool Done()
    {
        if (Source == null)
        {
            Source = AssignedEntity;
        }
        else if (Target == null)
        {
            Target = AssignedEntity;
        }

        (Source as CreatureData)?.Face(Target.Cell);

        if (SubTasksComplete())
        {
            if (AmountToChannel <= 0)
            {
                return true;
            }
            else
            {
                Source.ManaPool.BurnMana(ManaColor, 1);
                Target.ManaPool.GainMana(ManaColor, 1);

                Game.LeyLineController.MakeChannellingLine(Source, Target, 5, GameConstants.ChannelDuration, ManaColor);
                CreatureData?.CreatureRenderer.DisplayChannel(ManaColor, GameConstants.ChannelDuration);
                AmountToChannel--;
                AddSubTask(new Wait(GameConstants.ChannelDuration, $"{ManaColor}!!"));
            }
        }

        return false;
    }
}