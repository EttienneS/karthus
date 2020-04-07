using UnityEngine;
using UnityEngine.UI;

public class SkillDisplay : MonoBehaviour
{
    public Skill Skill;

    public Text Title;
    public Text Level;

    public Text Description;

    public Toggle Toggle;

    public Slider Slider;
    public Text Priority;

    private void Update()
    {
        if (Skill != null)
        {
            Skill.Enabled = Toggle.enabled;
            Skill.Priority = (int)Slider.value;

            Priority.text = Skill.Priority.ToString();
            Level.text = $"({Skill.Level})";
        }
    }

    internal void Load(Skill skill)
    {
        Skill = skill;
        Toggle.enabled = skill.Enabled;
        Slider.value = skill.Priority;

        Title.text = Skill.Name;
        Description.text = Skill.Description;
    }
}