using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class Instrumenter : IDisposable
{
    private Instrumenter(string name)
    {
        Stopwatch = new Stopwatch();
        Stopwatch.Start();
        Name = name;
    }

    public string Name { get; set; }
    public Stopwatch Stopwatch { get; set; }

    public static Instrumenter Init()
    {
        var stackTrace = new StackTrace();
        var frame = stackTrace.GetFrames()[1];
        var method = frame.GetMethod();

        return new Instrumenter($"[{method.DeclaringType.Name}.{method.Name}]");
    }

    public void Dispose()
    {
        Stamp("Completed");
    }

    public void Stamp(string message = "Stamp")
    {
        Debug.Log($"{Name} {message}:{Stopwatch.ElapsedMilliseconds}");
    }
}