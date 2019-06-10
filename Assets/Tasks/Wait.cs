﻿using UnityEngine;

public class Wait : TaskBase
{
    public float Duration;
    public float ElapsedTime;
    public float LastFacingChange;
    public string Reason;
    public bool Emote;

    public Wait()
    {
    }

    public Wait(float duration, string reason, bool emote = false)
    {
        Duration = duration;
        Reason = reason;
        ElapsedTime = 0;
        LastFacingChange = 0;
        Emote = emote;
        Message = $"{Reason} {Duration}";
    }

    public override bool Done()
    {
        if (Emote)
        {
            ShowDoneEmote();
        }
        return ElapsedTime >= Duration;
    }

    public override void Update()
    {
        if (Emote)
        {
            ShowBusyEmote();
        }
        ElapsedTime += Time.deltaTime;
    }
}