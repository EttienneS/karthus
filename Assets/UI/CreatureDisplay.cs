using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreatureDisplay : MonoBehaviour
{
    public CreatureIcon CreatureIconPrefab;

    public Dictionary<Creature, CreatureIcon> IconLookup;

    // Update is called once per frame
    void Update()
    {
        if (IconLookup == null)
        {
            IconLookup = new Dictionary<Creature, CreatureIcon>();

            foreach (var creature in Game.FactionController.PlayerFaction.Creatures)
            {
                AddIcon(creature);
            }
        }
        else
        {
            foreach (var creature in Game.FactionController.PlayerFaction.Creatures)
            {
                if (!IconLookup.ContainsKey(creature))
                {
                    AddIcon(creature);
                }
            }

            foreach (var creature in IconLookup.Keys.ToList())
            {
                if (!Game.FactionController.PlayerFaction.Creatures.Contains(creature))
                {
                    IconLookup.Remove(creature);
                }
            }
        }
    }

    private void AddIcon(Creature creature)
    {
        var icon = Instantiate(CreatureIconPrefab, transform);
        icon.Creature = creature;
        IconLookup.Add(creature, icon);
    }
}
