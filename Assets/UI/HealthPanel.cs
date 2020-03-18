using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HealthPanel : MonoBehaviour
{
    public LimbDisplay ProgressBarPrefab;

    internal Dictionary<Limb, LimbDisplay> LimbLinks;
    internal Creature Current;

    private void Update()
    {
        var creature = Game.CreatureInfoPanel.CurrentCreatures.OfType<Creature>().FirstOrDefault();

        if (creature == null)
        {
            return;
        }

        if (Current != creature)
        {
            Current = creature;
            LimbLinks = new Dictionary<Limb, LimbDisplay>();

            foreach (var limb in Current.Limbs)
            {
                var bar = Instantiate(ProgressBarPrefab, transform);
                bar.Limb = limb;
                LimbLinks.Add(limb, bar);
            }
        }
    }
}