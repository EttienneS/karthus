using System.Collections.Generic;

public static class FactionController
{
    internal static Dictionary<string, Faction> Factions = new Dictionary<string, Faction>();

    public static Faction PlayerFaction
    {
        get
        {
            return Factions[FactionConstants.Player];
        }
    }

    public static Faction MonsterFaction
    {
        get
        {
            return Factions[FactionConstants.Monster];
        }
    }

    public static Faction WorldFaction
    {
        get
        {
            return Factions[FactionConstants.World];
        }
    }
}