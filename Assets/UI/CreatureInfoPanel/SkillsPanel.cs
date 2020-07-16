using Assets.Creature;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillsPanel : MonoBehaviour
{
    public SkillDisplay SkillPrefab;

    public CreatureData Current;
    internal Dictionary<Skill, SkillDisplay> SkillLinks = new Dictionary<Skill, SkillDisplay>();

    public void Load(CreatureData creature)
    {
        if (Current != creature)
        {
            Current = creature;

            foreach (var prefab in SkillLinks.Values.ToList())
            {
                Destroy(prefab.gameObject);
            }

            SkillLinks = new Dictionary<Skill, SkillDisplay>();

            foreach (var skill in Current.Skills)
            {
                var skillDisplay = Instantiate(SkillPrefab, transform);
                skillDisplay.Load(skill);
                SkillLinks.Add(skill, skillDisplay);
            }
        }
    }
}