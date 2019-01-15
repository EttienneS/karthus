using System.Collections.Generic;
using UnityEngine;

public class Wait : ITask
{
    public float Duration;
    public float ElapsedTime;
    public float LastFacingChange;

    public Creature Creature { get; set; }

    public Wait(float duration)
    {
        Duration = duration;
        TaskId = $"Wait for {Duration}";
        ElapsedTime = 0;
        LastFacingChange = 0;
    }

    public Queue<ITask> SubTasks { get; set; }
    public string TaskId { get; set; }

    public bool Done()
    {
        return ElapsedTime >= Duration;
    }


    public override string ToString()
    {
        return $"Waiting for {ElapsedTime:F2}/{Duration:F2}";
    }


    public void Update()
    {
        ElapsedTime += Time.deltaTime;
        LastFacingChange += Time.deltaTime;

        if (LastFacingChange > 0.2f && Random.value > 0.95f)
        {
            Creature.SpriteAnimator.FaceRandomDirection();
            LastFacingChange = 0;
        }
    }
}