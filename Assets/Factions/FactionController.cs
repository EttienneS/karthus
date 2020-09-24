using Assets.ServiceLocator;
using System.Collections.Generic;
using UnityEngine;

public class FactionController : MonoBehaviour, IGameService
{
    internal Dictionary<string, Faction> Factions = new Dictionary<string, Faction>();

    public Faction MonsterFaction
    {
        get
        {
            return Factions[FactionConstants.Monster];
        }
    }

    public Faction PlayerFaction
    {
        get
        {
            return Factions[FactionConstants.Player];
        }
    }

    public Faction WorldFaction
    {
        get
        {
            return Factions[FactionConstants.World];
        }
    }

    public void Initialize()
    {
        if (SaveManager.SaveToLoad == null)
        {
            foreach (var factionName in new[]
            {
                FactionConstants.Player,
                FactionConstants.Monster,
                FactionConstants.World
            })
            {
                var faction = new Faction
                {
                    FactionName = factionName
                };
                Factions.Add(factionName, faction);
            }
        }
    }

    public void Update()
    {
        foreach (var faction in Factions)
        {
            faction.Value.Update();
        }
    }
}