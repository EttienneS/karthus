using Needs;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class NeedsPanel : MonoBehaviour
{
    public TitledProgressBar ProgressBarPrefab;

    internal Dictionary<NeedBase, TitledProgressBar> NeedProgressLinks = new Dictionary<NeedBase, TitledProgressBar>();
    internal Creature Current;

    private void Update()
    {
        var creature = Game.Instance.CreatureInfoPanel.CurrentCreatures.OfType<Creature>().FirstOrDefault();

        if (creature == null)
        {
            return;
        }

        if (Current != creature)
        {
            Current = creature;

            foreach (var prefab in NeedProgressLinks.Values.ToList())
            {
                Destroy(prefab.gameObject);
            }

            NeedProgressLinks = new Dictionary<NeedBase, TitledProgressBar>();

            foreach (var need in Current.Needs)
            {
                var bar = Instantiate(ProgressBarPrefab, transform);
                bar.Load(need.Name, need.Current / need.Max, need.Name, need.GetDescription());
                NeedProgressLinks.Add(need, bar);
            }
        }

        foreach (var need in creature.Needs)
        {
            NeedProgressLinks[need].SetProgress(need.Current / need.Max);
        }
    }
}