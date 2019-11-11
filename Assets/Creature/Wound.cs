using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

    public override string ToString()
    {
        return $"{GetName()} from {Source}";
    }

    public void Update(float delta)
    {
    }

    public Wound(Limb limb, string source, DamageType damageType, Severity severity)
    {
        Limb = limb;
        Source = source;
        DamageType = damageType;
        Severity = severity;
    }
}