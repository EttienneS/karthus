using Newtonsoft.Json;

public interface IEntity
{
    Cell Cell { get; set; }

    string Name { get; set; }
    string Id { get; set; }

    ManaPool ManaPool { get; set; }

    string FactionName { get; set; }

    [JsonIgnore]
    Task Task { get; set; }

    void Damage(int amount, ManaColor type);
}

public static class EntityHelpers
{
    public static Faction GetFaction(this IEntity entity)
    {
        return FactionController.Factions[entity.FactionName];
    }
}