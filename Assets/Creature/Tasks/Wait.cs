using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Wait : TaskBase
{
    public float Duration;
    public float ElapsedTime;
    public float LastFacingChange;
    public string Reason;

    public Wait()
    {
    }

    public Wait(float duration, string reason)
    {
        Duration = duration;
        Reason = reason;
        ElapsedTime = 0;
        LastFacingChange = 0;
    }

    public override bool Done()
    {
        return ElapsedTime >= Duration;
    }

    public override string ToString()
    {
        return $"{Reason} {Duration}";
    }

    public override void Update()
    {
        ElapsedTime += Time.deltaTime;
        LastFacingChange += Time.deltaTime;

        if (LastFacingChange > 0.2f && Random.value > 0.95f)
        {
            Creature.LinkedGameObject.FaceRandomDirection();
            LastFacingChange = 0;
        }
    }
}