using System.Collections.Generic;
using UnityEngine;

public class Wait : CreatureTask
{
    public override Dictionary<ManaColor, float> Cost => new Dictionary<ManaColor, float>();
    public float Duration;
    public float ElapsedTime;
    public string Reason;

    public Wait()
    {
    }

    public Wait(float duration, string reason) : this()
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