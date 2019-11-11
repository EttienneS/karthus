using System.Collections.Generic;
using UnityEngine;

public enum TargetType
{
    Easy, Critical, Biggest
}

public interface IEntity
{
    Cell Cell { get; set; }
    string FactionName { get; set; }
    string Id { get; set; }
    List<VisualEffectData> LinkedVisualEffects { get; set; }
    List<string> LogHistory { get; set; }
    ManaPool ManaPool { get; set; }
    string Name { get; set; }
    Dictionary<string, string> Properties { get; set; }
    Dictionary<string, float> ValueProperties { get; set; }
    Vector2 Vector { get; }

    void Log(string message);
}