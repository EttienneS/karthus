public static class EntityHelpers
{
    public static Faction GetFaction(this IEntity entity)
    {
        return Game.Instance.FactionController.Factions[entity.FactionName];
    }

    public static bool IsPlayerControlled(this IEntity entity)
    {
        return entity.FactionName == FactionConstants.Player;
    }
}