using Newtonsoft.Json;

public class Channel : CreatureTask
{
    public int AmountToChannel;

    public ManaColor ManaColor;

    public string SourceId;

    public string TargetId;

    public Channel()
    {
    }

    [JsonIgnore]
    public IEntity Source
    {
        get
        {
            return IdService.GetEntity(SourceId);
        }
    }

    [JsonIgnore]
    public IEntity Target
    {
        get
        {
            return IdService.GetEntity(TargetId);
        }
    }

    public static Channel GetChannelFrom(ManaColor color, int amount, IEntity source)
    {
        var task = new Channel
        {
            ManaColor = color,
            AmountToChannel = amount,
            SourceId = source.Id
        };

        task.AddSubTask(new Move(Game.Map.GetPathableNeighbour(source.Cell)));

        return task;
    }

    public static Channel GetChannelTo(ManaColor color, int amount, IEntity target)
    {
        var task = new Channel
        {
            ManaColor = color,
            AmountToChannel = amount,
            TargetId = target.Id
        };

        task.AddSubTask(new Move(Game.Map.GetPathableNeighbour(target.Cell)));

        return task;
    }

    public override bool Done(Creature creature)
    {
        if (string.IsNullOrEmpty(SourceId))
        {
            SourceId = creature.Id;
        }
        else if (string.IsNullOrEmpty(TargetId))
        {
            TargetId = creature.Id;
        }

        (Source as Creature)?.Face(Target.Cell);
        (Target as Creature)?.Face(Source.Cell);

        if (SubTasksComplete(creature))
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
                creature.CreatureRenderer.DisplayChannel(ManaColor, GameConstants.ChannelDuration);
                AmountToChannel--;
                AddSubTask(new Wait(GameConstants.ChannelDuration, $"{ManaColor}!!"));
            }
        }

        return false;
    }
}