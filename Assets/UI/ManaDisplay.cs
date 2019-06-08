using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ManaDisplay : MonoBehaviour
{
    public Text TextPrefab;

    public Dictionary<Mana, Tuple<Text, Text>> LabelDictionary = new Dictionary<Mana, Tuple<Text, Text>>();

    public void EnsureDisplay(Mana mana)
    {
        if (!LabelDictionary.ContainsKey(mana))
        {
            var title = Instantiate(TextPrefab);
            var value = Instantiate(TextPrefab);
            LabelDictionary.Add(mana, Tuple.Create(title, value));
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