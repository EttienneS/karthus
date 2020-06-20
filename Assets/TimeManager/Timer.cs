using System;

[Serializable]
public class Timer
{
    internal bool CanRemove;
    public int Elapsed;
    public int TotalMinutes;

    public Timer(int totalMinutes)
    {
        TotalMinutes = totalMinutes;
        Elapsed = 0;
    }

    public bool IsDone()
    {
        CanRemove = Elapsed >= TotalMinutes;
        return CanRemove;
    }
}
