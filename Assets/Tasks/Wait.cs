using UnityEngine;
using Assets.Creature;
using Assets.ServiceLocator;

public class Wait : CreatureTask
{
    public int TimerId;
    public string Reason;
    public AnimationType Animation;

    public override string Message
    {
        get
        {
            var timer = GetTimer();
            return $"{Reason}: {timer.Elapsed}/{timer.TotalMinutes}";
        }
    }

    public Wait()
    {
    }

    public override void FinalizeTask()
    {
    }

    public Wait(int duration, string reason, AnimationType animation = AnimationType.Idle) : this()
    {
        Animation = animation;
        TimerId = Loc.GetTimeManager().StartTimer(duration);
        Reason = reason;
    }

    public Timer GetTimer()
    {
        return Loc.GetTimeManager().GetTimer(TimerId);
    }

    public override bool Done(CreatureData creature)
    {
        creature.SetAnimation(Animation);
        if (GetTimer().IsDone())
        {
            ShowDoneEmote(creature);
            return true;
        }
        return false;
    }
}