using UnityEngine;

public class Interact : EntityTask
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

    public override bool Done()
    {
        if (Interactor == null)
        {
            Effect.AssignedEntity = Interactor;
        }
        else
        {
            Effect.AssignedEntity = AssignedEntity;
        }

        Effect.Target = Target;

        if (Target is Structure structure)
        {
            structure.Reserve(Interactor);
        }

        if (SubTasksComplete() && Effect.Done())
        {
            Target = null;
            return true;
        }
        return false;
    }
}