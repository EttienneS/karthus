using UnityEngine;

public class Interact : EntityTask
{
    public float Duration;
    public float ElapsedTime;
    public IEntity Target;

    public EffectBase Effect;

    public Interact()
    {
    }

    public Interact(float duration, IEntity target, EffectBase effect)
    {
        Duration = duration;
        ElapsedTime = 0;
        Target = target;
        Effect = effect;
    }

    public override bool Done()
    {
        ElapsedTime += Time.deltaTime;

        if (ElapsedTime >= Duration)
        {
            ShowDoneEmote();
            return true;
        }
        return false;
    }
}