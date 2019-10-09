using UnityEngine;

public class Interact : CreatureTask
{
    public EffectBase Effect;

    public IEntity Interactor;
    public IEntity Target;

    public Interact()
    {
    }

    public Interact(EffectBase effect, IEntity interactor, IEntity target)
    {
        Effect = effect;
        Target = target;
        Interactor = interactor;
    }

    public override bool Done(CreatureData creature)
    {
        if (Interactor == null)
        {
            Effect.AssignedEntity = Interactor;
        }
        else
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
            Target = null;
            return true;
        }
        return false;
    }
}