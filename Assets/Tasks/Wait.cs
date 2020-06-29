using UnityEngine;

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

    public override void Complete()
    {
    }

    public Wait(int duration, string reason, AnimationType animation = AnimationType.Idle) : this()
    {
        Animation = animation;
        TimerId = Game.Instance.TimeManager.StartTimer(duration);
        Reason = reason;
    }

    public Timer GetTimer()
    {
        return Game.Instance.TimeManager.GetTimer(TimerId);
    }

    public override bool Done(Creature creature)
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