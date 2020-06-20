using System;
using System.Collections.Generic;

[Serializable]
public class TimeData
{
    public int Hour;
    public int LastId;
    public int Minute;
    public Dictionary<int, Timer> Timers = new Dictionary<int, Timer>();

    public Timer GetTimer(int id)
    {
        return Timers[id];
    }

    internal int CreateTimer(int totalMinutes)
    {
        var timer = new Timer(totalMinutes);
        var id = ++LastId; // assign and increment last ID and then assign

        Timers.Add(id, timer);

        return id;
    }

    internal void UpdateTimers()
    {
        var removes = new List<int>();
        foreach (var kvp in Timers)
        {
            kvp.Value.Elapsed++;
            if (kvp.Value.CanRemove)
            {
                removes.Add(kvp.Key);
            }
        }

        foreach(var toRemove in removes)
        {
            Timers.Remove(toRemove);
        }
    }
}