using UnityEngine;

public class WaitTask : ITask
{
    public float Duration;
    public string TaskId { get; set; }

    public float ElapsedTime;

    public WaitTask(float duration)
    {
        Duration = duration;
        TaskId = $"Wait for {Duration}";
    }

    public override string ToString()
    {
        return $"Waiting for {ElapsedTime:F2}/{Duration:F2}";
    }

    public void Start(Creature creature)
    {
        ElapsedTime = 0;
    }

    public bool Done(Creature creature)
    {
        return ElapsedTime >= Duration;
    }

    public void Update(Creature creature)
    {
        ElapsedTime += Time.deltaTime;
    }
}