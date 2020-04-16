using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class HealthPanel : MonoBehaviour
{
    public TitledProgressBar ProgressBarPrefab;

    internal Dictionary<Limb, TitledProgressBar> LimbLinks = new Dictionary<Limb, TitledProgressBar>();
    internal Creature Current;

    private void Update()
    {
        var creature = Game.Instance.SelectedCreatures.FirstOrDefault();

        if (creature == null)
        {
            return;
        }

        if (Current != creature.Data)
        {
            Current = creature.Data;

            foreach (var prefab in LimbLinks.Values.ToList())
            {
                Destroy(prefab.gameObject);
            }

            LimbLinks = new Dictionary<Limb, TitledProgressBar>();

            foreach (var limb in Current.Limbs)
            {
                var bar = Instantiate(ProgressBarPrefab, transform);
                bar.Load(limb.Name, limb.State, limb.Name, limb.ToString());
                LimbLinks.Add(limb, bar);
            }
        }

        foreach (var limb in Current.Limbs)
        {
            LimbLinks[limb].SetProgress(limb.State);
            LimbLinks[limb].TooltipText = limb.ToString();
        }
    }
}