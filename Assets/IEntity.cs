using System.Collections.Generic;
using Newtonsoft.Json;

public interface IEntity
{
    Cell Cell { get; set; }

    string Name { get; set; }
    string Id { get; set; }

    ManaPool ManaPool { get; set; }

    string FactionName { get; set; }

    [JsonIgnore]
    EntityTask Task { get; set; }

    void Damage(int amount, ManaColor type);

    Dictionary<string, string> Properties { get; set; }
    Dictionary<string, float> ValueProperties { get; set; }
}

public static class EntityHelpers
{
    public static Faction GetFaction(this IEntity entity)
    {
        return FactionController.Factions[entity.FactionName];
    }
}