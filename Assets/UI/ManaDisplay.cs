using System;
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
            value.color = mana.GetActualColor();
            LabelDictionary.Add(mana, value);
        }
    }

    private void Update()
    {
        foreach (var kvp in LabelDictionary)
        {
            var mana = kvp.Key;
            var labels = kvp.Value;

            labels.text = mana.Total.ToString();
        }
    }
}