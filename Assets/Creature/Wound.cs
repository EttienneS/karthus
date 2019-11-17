using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

[JsonConverter(typeof(StringEnumConverter))]
public enum Severity
{
    Low, Medium, High, Critical
}

public class Wound
{
    public DamageType DamageType;
    public Severity Severity;
    public string Source;

    public Wound(Limb limb, string source, DamageType damageType, Severity severity)
    {
        Limb = limb;
        Source = source;
        DamageType = damageType;
        Severity = severity;
    }

    public float Age { get; set; }

    [JsonIgnore]
    public int Danger
    {
        get
        {
            var danger = 2;
            switch (Severity)
            {
                case Severity.Low:
                    danger = 2;
                    break;

                case Severity.Medium:
                    danger = 4;
                    break;
                case Severity.High:
                    danger = 10;
                    break;
                case Severity.Critical:
                    danger = 30;
                    break;
            }

            if (Treated)
            {
                danger /= 2;
            }

            return danger;

        }
    }

    public float HealRate { get; set; } = 5;

    [JsonIgnore]
    public Limb Limb { get; set; }

    [JsonIgnore]
    public Creature Owner
    {
        get
        {
            return Limb.Owner;
        }
    }

    public bool Treated { get; set; }

    public string GetName()
    {
        switch (DamageType)
        {
            case DamageType.Bludgeoning:
                switch (Severity)
                {
                    case Severity.Low:
                        return "Bruise";

                    case Severity.Medium:
                        return "Severe Bruise";

                    case Severity.High:
                        return "Fracture";

                    case Severity.Critical:
                        return "Break";
                }
                break;

            case DamageType.Slashing:
                switch (Severity)
                {
                    case Severity.Low:
                        return "Scrape";

                    case Severity.Medium:
                        return "Shallow Cut";

                    case Severity.High:
                        return "Cut";

                    case Severity.Critical:
                        return "Sever";
                }
                break;

            case DamageType.Piercing:
                switch (Severity)
                {
                    case Severity.Low:
                        return "Nick";

                    case Severity.Medium:
                        return "Shallow Puncture";

                    case Severity.High:
                        return "Puncture";

                    case Severity.Critical:
                        return "Impaled";
                }
                break;

            case DamageType.Energy:
                switch (Severity)
                {
                    case Severity.Low:
                        return "Abrasion";

                    case Severity.Medium:
                        return "1st Degree Burn";

                    case Severity.High:
                        return "2nd Degree Burn";

                    case Severity.Critical:
                        return "3rd Degree Burn";
                }
                break;
        }

        return $"?? {DamageType.Energy} ??";
    }

    public bool Healed()
    {
        var healed = false;
        var name = GetName();

        var target = HealRate * Danger;


        if (Age > target)
        {
            Age = 0;

            if (Severity == Severity.Low)
            {
                Owner.Log($"{name} healed completely.");
                // leave a scar?
                return true;
            }
            else
            {
                Severity--;
                Owner.Log($"{name} healed to a {GetName()}");
            }
        }


        return healed;
    }

    public override string ToString()
    {
        var status = Treated ? "Treated" : "Not Treated";
        return $"{GetName()} from {Source} [{status}]";
    }

    public void Update(float delta)
    {
        Age += delta;
    }
}