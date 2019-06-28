using System.Collections.Generic;

public static class FactionController
{
    public static Dictionary<string, Faction> Factions = new Dictionary<string, Faction>();

    public static Faction PlayerFaction
    {
        get
        {
            return Factions[FactionConstants.Player];
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