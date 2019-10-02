﻿using UnityEngine;

public class Wait : EntityTask
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