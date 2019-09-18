using UnityEngine;

public class Wait : EntityTask
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
            ShowBusyEmote();
        }
        ElapsedTime += Time.deltaTime;

        if (ElapsedTime >= Duration)
        {
            ShowDoneEmote();
            return true;
        }
        return false;
    }
}