using System.Collections.Generic;
using UnityEngine;

public interface IEntity
{
    Cell Cell { get; set; }
    string FactionName { get; set; }
    string Id { get; set; }
    List<VisualEffectData> LinkedVisualEffects { get; set; }
    string Name { get; set; }
    Dictionary<string, string> Properties { get; set; }
    Dictionary<string, float> ValueProperties { get; set; }
    Vector2 Vector { get; }
}

public static class NamedProperties
{
    public const string ContainedItemIds = "ContainedItemIds";
}
