﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaDisplay : MonoBehaviour
{
    public Dictionary<Mana, Text> LabelDictionary = new Dictionary<Mana, Text>();
    public Text TextPrefab;

    public void EnsureDisplay(Mana mana)
    {
        if (!LabelDictionary.ContainsKey(mana))
        {
            var value = Instantiate(TextPrefab, transform);
            value.name = $"{mana.Color} value";
            value.color = mana.Color.GetActualColor();
            LabelDictionary.Add(mana, value);
        }
    }

    private void Update()
    {
        foreach (var kvp in LabelDictionary)
        {
            var mana = kvp.Key;
            var label = kvp.Value;
            var floatingMana = 0;

            foreach (var creature in FactionController.PlayerFaction.Creatures)
            {
                if (creature.ManaPool.ContainsKey(mana.Color))
                {
                    floatingMana += creature.ManaPool[mana.Color].Total;
                }
            }

            label.text = $"{mana.Total} ({floatingMana})";
        }
    }

    internal void EnsureDisplay(ManaPool manaPool)
    {
        foreach (var mana in manaPool)
        {
            EnsureDisplay(mana.Value);
        }
    }
}