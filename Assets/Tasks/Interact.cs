using UnityEngine;

public class Interact : EntityTask
{
    public EffectBase Effect;

    public IEntity Target;

    public Interact()
    {
    }

    public Interact(EffectBase effect, IEntity target)
    {
        Effect = effect;
        Target = target;

    }

    public override bool Done()
    {
        if (Effect.AssignedEntity == null)
        {
            Effect.Originator = Originator;
            Effect.AssignedEntity = AssignedEntity;
            Effect.Target = Target;
        }

        if (SubTasksComplete() && Effect.Ready())
        {
            return Effect.DoEffect();
        }
        return false;
    }
}