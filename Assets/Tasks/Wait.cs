using UnityEngine;
using Random = UnityEngine.Random;

public class Wait : TaskBase
{
    public float Duration;
    public float ElapsedTime;
    public float LastFacingChange;
    public bool ChangeFacing;
    public string Reason;

    public Wait()
    {
    }

    public Wait(float duration, string reason, bool changeFacing = false)
    {
        Duration = duration;
        Reason = reason;
        ElapsedTime = 0;
        LastFacingChange = 0;
        ChangeFacing = changeFacing;

        Message = $"{Reason} {Duration}";
    }

    public override bool Done()
    {
        return ElapsedTime >= Duration;
    }

    public override void Update()
    {
        ElapsedTime += Time.deltaTime;

        //if (ChangeFacing)
        //{
        //    LastFacingChange += Time.deltaTime;

        //    if (LastFacingChange > 0.2f && Random.value > 0.95f)
        //    {
        //        Creature.LinkedGameObject.FaceRandomDirection();
        //        LastFacingChange = 0;
        //    }
        //}
    }
}