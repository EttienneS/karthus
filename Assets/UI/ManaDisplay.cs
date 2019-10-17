using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaDisplay : MonoBehaviour
{
    public Dictionary<ManaColor, Text> LabelDictionary = new Dictionary<ManaColor, Text>();
    public Text TextPrefab;
    public bool _runOnce;

    private void Update()
    {
        if (Game.TimeManager.Paused)
        {
            return;
        }

        if (!_runOnce)
        {
            foreach (ManaColor mana in Enum.GetValues(typeof(ManaColor)))
            {
                var value = Instantiate(TextPrefab, transform);
                value.name = $"{mana} value";
                value.color = mana.GetActualColor();
                LabelDictionary.Add(mana, value);
            }
            _runOnce = true;
        }

        foreach (var kvp in LabelDictionary)
        {
            var mana = kvp.Key;
            var label = kvp.Value;
            var floatingMana = 0;
            var stored = 0;

            foreach (var creature in Game.FactionController.PlayerFaction.Creatures)
            {
                if (creature.ManaPool.ContainsKey(mana))
                {
                    floatingMana += creature.ManaPool[mana].Total;
                }
            }

            foreach (var structure in Game.FactionController.PlayerFaction.Structures)
            {
                if (structure.IsType("Battery"))
                {
                    stored += structure.ManaPool[mana].Total;
                }
            }

            label.text = $"{stored} ({floatingMana})";
        }
    }
}