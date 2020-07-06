using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Creature;

public class CreatureDisplay : MonoBehaviour
{
    public CreatureIcon CreatureIconPrefab;

    public Dictionary<CreatureData, CreatureIcon> IconLookup;

    // Update is called once per frame
    void Update()
    {
        if (IconLookup == null)
        {
            IconLookup = new Dictionary<CreatureData, CreatureIcon>();

            foreach (var creature in Game.Instance.FactionController.PlayerFaction.Creatures)
            {
                AddIcon(creature);
            }
        }
        else
        {
            foreach (var creature in Game.Instance.FactionController.PlayerFaction.Creatures)
            {
                if (!IconLookup.ContainsKey(creature))
                {
                    AddIcon(creature);
                }
            }

            foreach (var creature in IconLookup.Keys.ToList())
            {
                if (!Game.Instance.FactionController.PlayerFaction.Creatures.Contains(creature))
                {
                    IconLookup.Remove(creature);
                }
            }
        }
    }

    private void AddIcon(CreatureData creature)
    {
        var icon = Instantiate(CreatureIconPrefab, transform);
        icon.Creature = creature;
        IconLookup.Add(creature, icon);
    }
}
