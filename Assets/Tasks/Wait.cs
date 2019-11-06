using UnityEngine;

public class Wait : CreatureTask
{
    public float Duration;
    public float ElapsedTime;
    public string Reason;

    public Wait()
    {
    }

    public Wait(float duration, string reason)
    {
        Duration = duration;
        Reason = reason;
        ElapsedTime = 0;
        Message = $"{Reason} {Duration}";
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