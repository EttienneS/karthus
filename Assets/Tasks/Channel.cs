using Newtonsoft.Json;

public class Channel : CreatureTask
{
    public float AmountToChannel;

    public ManaColor ManaColor;

    public string SourceId;

    public string TargetId;

    public Channel()
    {
        RequiredSkill = "Arcana";
        RequiredSkillLevel = 1;
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

    public static Channel GetChannelFrom(ManaColor color, float amount, IEntity source)
    {
        return new Channel
        {
            ManaColor = color,
            AmountToChannel = amount,
            SourceId = source.Id
        };
    }

    public static Channel GetChannelTo(ManaColor color, float amount, IEntity target)
    {
        return new Channel
        {
            ManaColor = color,
            AmountToChannel = amount,
            TargetId = target.Id
        };
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

        if (SubTasksComplete(creature))
        {
            if (Source.Cell.DistanceTo(Target.Cell) > 2)
            {
                AddSubTask(new Move(Game.Map.GetPathableNeighbour(creature == Source ? Target.Cell : Source.Cell)));
            }
            else if (AmountToChannel <= 0)
            {
                return true;
            }
            else
            {
                if (Source is Creature sourceCreature)
                {
                    sourceCreature.Face(Target.Cell);
                    sourceCreature.ManaPool.BurnMana(ManaColor, 1);
                }
                else
                {
                    Source.ManaValue.BurnMana(ManaColor, 1);
                }

                if (Target is Creature targetCreature)
                {
                    targetCreature.Face(Source.Cell);
                    targetCreature.ManaPool.GainMana(ManaColor, 1);
                }
                else
                {
                    Target.ManaValue.GainMana(ManaColor, 1);
                }

                Game.VisualEffectController.MakeChannellingLine(Source, Target, 5, GameConstants.ChannelDuration, ManaColor);
                creature.CreatureRenderer.DisplayChannel(ManaColor, GameConstants.ChannelDuration);
                AmountToChannel--;
                AddSubTask(new Wait(GameConstants.ChannelDuration, $"{ManaColor}!!"));
            }
        }

        return false;
    }
}