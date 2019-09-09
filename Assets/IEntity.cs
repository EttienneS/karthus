using Newtonsoft.Json;

public interface IEntity
{
    CellData Cell { get; set; }

    string Id { get; set; }

    ManaPool ManaPool { get; set; }

    string FactionName { get; set; }

    [JsonIgnore]
    TaskBase Task { get; set; }

    void Damage(int amount, ManaColor type);
}

public static class EntityHelpers
{
    public static Faction GetFaction(this IEntity entity)
    {
        return FactionController.Factions[entity.FactionName];
    }
}