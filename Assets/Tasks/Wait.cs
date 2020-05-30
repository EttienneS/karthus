using UnityEngine;

public class Wait : CreatureTask
{
    public float Duration;
    public float ElapsedTime;
    public string Reason;

    public override string Message
    {
        get
        {
            return $"{Reason} {Duration}";
        }
    }

    public Wait()
    {
    }

    public override void Complete()
    {
    }

    public Wait(float duration, string reason) : this()
    {
        Duration = duration;
        Reason = reason;
        ElapsedTime = 0;
    }

    public override bool Done(Creature creature)
    {
        ElapsedTime += Time.deltaTime;

        if (ElapsedTime >= Duration)
        {
            ShowDoneEmote(creature);
            return true;
        }
        return false;
    }
}