﻿using Newtonsoft.Json;
using UnityEngine;

public class Interact : CreatureTask
{
    public EffectBase Effect;

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
            return IdService.GetEntity(TargetID); ;
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
        InteractorID = interactor.Id;
    }

    public override bool Done(CreatureData creature)
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