using Newtonsoft.Json;
using System.Collections.Generic;

public class Interact : CreatureTask
{
    public EffectBase Effect;
    public override Dictionary<ManaColor, float> Cost => new Dictionary<ManaColor, float>();

    [JsonIgnore]
    public IEntity Interactor
    {
        get
        {
            return IdService.GetEntity(InteractorID);
        }
    }

    public string InteractorID;

    [JsonIgnore]
    public IEntity Target
    {
        get
        {
            return IdService.GetEntity(TargetID);
        }
    }

    public string TargetID;

    public Interact()
    {
    }

    public Interact(EffectBase effect, IEntity interactor, string targetID) : this()
    {
        Effect = effect;
        TargetID = targetID;
        InteractorID = interactor.Id;
    }

    public override bool Done(Creature creature)
    {
        if (string.IsNullOrEmpty(InteractorID))
        {
            InteractorID = creature.Id;
        }

        if (string.IsNullOrEmpty(Effect.AssignedEntityId))
        {
            Effect.AssignedEntityId = creature.Id;
        }

        Effect.TargetId = Target.Id;

        if (Target is Structure structure)
        {
            structure.Reserve(Interactor);
        }

        if (SubTasksComplete(creature) && Effect.Done())
        {
            TargetID = null;
            return true;
        }
        return false;
    }
}