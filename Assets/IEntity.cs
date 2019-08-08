using Newtonsoft.Json;

public interface IEntity
{
    Coordinates Coordinates { get; set; }

    string Id { get; set; }

    ManaPool ManaPool { get; set; }

    string FactionName { get; set; }

    [JsonIgnore]
    TaskBase Task { get; set; }
}

public static class EntityHelpers
{
    public static Faction GetFaction(this IEntity entity)
    {
        return FactionController.Factions[entity.FactionName];
    }
}