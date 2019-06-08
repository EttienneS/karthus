using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaDisplay : MonoBehaviour
{
    public Dictionary<Mana, Tuple<Text, Text>> LabelDictionary = new Dictionary<Mana, Tuple<Text, Text>>();
    public Text TextPrefab;

    public void EnsureDisplay(Mana mana)
    {
        if (!LabelDictionary.ContainsKey(mana))
        {
            LabelDictionary.Add(mana,
                                Tuple.Create(
                                    Instantiate(TextPrefab, transform),
                                    Instantiate(TextPrefab, transform)));
        }
    }

    private void Update()
    {
        foreach (var kvp in LabelDictionary)
        {
            var mana = kvp.Key;
            var labels = kvp.Value;

            labels.Item1.text = mana.Color.ToString();
            labels.Item2.text = mana.Total.ToString();
        }
    }
}