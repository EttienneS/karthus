using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Creature;

public class SkillsPanel : MonoBehaviour
{
    public SkillDisplay SkillPrefab;

    public CreatureData Current;
    internal Dictionary<Skill, SkillDisplay> SkillLinks = new Dictionary<Skill, SkillDisplay>();

    private void Update()
    {
        var creature = Game.Instance.Cursor.SelectedCreatures.FirstOrDefault();

        if (creature == null)
        {
            return;
        }

        if (Current != creature.Data)
        {
            Current = creature.Data;

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