using UnityEngine;
using Animation = LPC.Spritesheet.Generator.Interfaces.Animation;

public class Wait : CreatureTask
{
    public float Duration;
    public float ElapsedTime;
    public string Reason;
    public Animation? Animation;

    private bool _hasRunOnce;

    public Wait()
    {
    }

    public override void Complete()
    {
    }

    public Wait(float duration, string reason, Animation? animation = LPC.Spritesheet.Generator.Interfaces.Animation.Walk) : this()
    {
        Duration = duration;
        Reason = reason;
        ElapsedTime = 0;
        Message = $"{Reason} {Duration}";
        Animation = animation;
    }

    public override bool Done(Creature creature)
    {
        if (!_hasRunOnce)
        {
            if (Animation.HasValue)
            {
                creature.SetAnimation(Animation.Value, Duration);
            }
            _hasRunOnce = true;
        }

        ElapsedTime += Time.deltaTime;

        if (ElapsedTime >= Duration)
        {
            ShowDoneEmote(creature);
            return true;
        }
        return false;
    }
}