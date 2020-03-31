using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Random = UnityEngine.Random;

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

    private bool _bleeding;

    private bool _infected;

    public Wound(Limb limb, string source, DamageType damageType, Severity severity)
    {
        Limb = limb;
        Source = source;
        DamageType = damageType;
        Severity = severity;
        Bleeding = false;
        Infected = false;

        if (damageType == DamageType.Piercing || DamageType == DamageType.Slashing)
        {
            // Chance of Bleed:
            // 10% on Low
            // 40% on Medium
            // 70% on High
            // 100% on Critical
            if (Random.value < (((int)Severity * 0.3f) + 0.1f))
            {
                Bleeding = true;
            }
        }
    }

    public float Age { get; set; }

    public bool Bleeding
    {
        get
        {
            return _bleeding;
        }
        set
        {
            if (_bleeding && !value)
            {
                Limb.Owner.Log($"The bleeding from {Source} has stopped.");
            }
            else if (!_bleeding && value)
            {
                Limb.Owner.Log($"The wound from {Source} is bleeding!");
            }
            _bleeding = value;
        }
    }

    [JsonIgnore]
    public int Danger
    {
        get
        {
            int danger;
            switch (Severity)
            {
                case Severity.Critical:
                    danger = 10;
                    break;

                case Severity.High:
                    danger = 6;
                    break;

                case Severity.Medium:
                    danger = 3;
                    break;

                default:
                case Severity.Low:
                    danger = 1;
                    break;
            }

            if (Infected)
            {
                danger *= 2;
            }

            if (Bleeding)
            {
                danger *= 2;
            }

            if (Treated)
            {
                danger /= 2;
            }

            return danger;
        }
    }

    public float HealRate { get; set; } = 5;

    public bool Infected
    {
        get
        {
            return _infected;
        }
        set
        {
            if (_infected && !value)
            {
                Limb.Owner.Log($"The infection from {Source} has run its course.");
            }
            else if (!_infected && value)
            {
                Limb.Owner.Log($"The wound from {Source} has become infected!");
            }
            _infected = value;
        }
    }

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

    private bool _treated;

    public bool Treated
    {
        get
        {
            return _treated;
        }
        set
        {
            if (value)
            {
                Limb.Owner.Log($"Treated the wound from {Source}!");

                if (Bleeding)
                {
                    Bleeding = false;
                }
            }
            else if (_treated && !value)
            {
                Limb.Owner.Log($"Treatment expired on {Source}!");
            }
            _treated = value;
        }
    }

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

        if (Age > StageHealedAge)
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
                Treated = false;
                Owner.Log($"{name} healed to a {GetName()}");
            }
        }

        return healed;
    }

    [JsonIgnore]
    public float StageHealedAge
    {
        get
        {
            return HealRate * Danger;
        }
    }

    [JsonIgnore]
    public int HealPercentage
    {
        get
        {
            return (int)(Age / StageHealedAge * 100);
        }
    }

    public override string ToString()
    {
        var status = Treated ? "Treated" : "Not Treated";
        if (Bleeding)
        {
            status += " - Bleeding -";
        }
        if (Infected)
        {
            status += " - Infected -";
        }

        return $"{GetName()} from {Source} ({HealPercentage}%) [{status}]";
    }

    public void Update(float delta)
    {
        Age += delta;
    }
}