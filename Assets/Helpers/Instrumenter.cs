using System;
using UnityEngine;


public class Instrumenter : IDisposable
{
    public System.Diagnostics.Stopwatch Stopwatch { get; set; }

    public string Name { get; set; }

    public Instrumenter(string name)
    {
        Stopwatch = new System.Diagnostics.Stopwatch();
        Stopwatch.Start();
        Name = name;
    }

    public void Stamp(string message = "Stamp")
    {
        Debug.Log($"{Name} {message}:{Stopwatch.ElapsedMilliseconds}");
    }

    public void Dispose()
    {
        Stamp("Done");
    }
}