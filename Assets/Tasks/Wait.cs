using UnityEngine;

public class Wait : CreatureTask
{
    public int TimerId;
    public string Reason;

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

    public Wait(int duration, string reason) : this()
    {
        TimerId = Game.Instance.TimeManager.StartTimer(duration);
        Reason = reason;
    }

    public Timer GetTimer()
    {
        return Game.Instance.TimeManager.GetTimer(TimerId);
    }

    public override bool Done(Creature creature)
    {
        if (GetTimer().IsDone())
        {
            ShowDoneEmote(creature);
            return true;
        }
        return false;
    }
}