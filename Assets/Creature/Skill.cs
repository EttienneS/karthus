using UnityEngine;

public class Skill
{
    private int _priority;

    public Skill(string name)
    {
        Name = name;
        Level = 0f;
        Enabled = true;
        Priority = 5;
    }

    public bool Enabled { get; set; } = true;
    public float Level { get; set; }
    public string Name { get; set; }

    public int Priority
    {
        get
        {
            return _priority;
        }
        set
        {
            _priority = Mathf.Clamp(value, 1, 10);
        }
    }

    public int RollSkill()
    {
        return Randomf.Roll(20) + Mathf.FloorToInt(Level);
    }

    public override string ToString()
    {
        var skill = $"{Name}: {Level.ToString("N2")} [{Priority}]";

        if (!Enabled)
        {
            skill = $"-{skill}-";
        }

        return skill;
    }
}