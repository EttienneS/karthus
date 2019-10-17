using System.Collections.Generic;

public static class EntityHelpers
{
    public static Faction GetFaction(this IEntity entity)
    {
        return Game.FactionController.Factions[entity.FactionName];
    }
}