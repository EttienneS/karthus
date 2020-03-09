using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NeedsPanel : MonoBehaviour
{
    public Text NeedsText;

    private void Update()
    {
        var creature = Game.CreatureInfoPanel.CurrentCreatures.OfType<Creature>().FirstOrDefault();

        if (creature == null)
        {
            return;
        }

        NeedsText.text = "\nNeeds:\n\n";
        foreach (var need in creature.Needs)
        {
            NeedsText.text += $"\t{need} [{need.CurrentChangeRate}]\n";
        }

        NeedsText.text += $"\nMood: {creature.MoodString} ({creature.Mood})\n";
        foreach (var feeling in creature.Feelings)
        {
            NeedsText.text += $"\t{feeling}\n";
        }
    }
}