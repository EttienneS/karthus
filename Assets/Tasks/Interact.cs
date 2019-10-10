using Newtonsoft.Json;
using UnityEngine;

public class Interact : CreatureTask
{
    public EffectBase Effect;

    public IEntity Interactor;

    public IEntity _target;

    [JsonIgnore]
    public IEntity Target
    {
        get
        {
            if (_target == null)
            {
                _target = IdService.GetEntityFromId(TargetID);
            }

            return _target;
        }
    }

    public string TargetID;

    public Interact()
    {
    }

    public Interact(EffectBase effect, IEntity interactor, string targetID)
    {
        Effect = effect;
        TargetID = targetID;
        Interactor = interactor;
    }

    public override bool Done(CreatureData creature)
    {
        if (Interactor == null)
        {
            Interactor = creature;
        }

        if (Effect.AssignedEntity == null)
        {
            Effect.AssignedEntity = creature;
        }

        Effect.Target = Target;

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