using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine;
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

    public int Danger
    {
        get
        {
            switch (Severity)
            {
                case Severity.Low:
                    return 2;

                case Severity.Medium:
                    return 4;

                case Severity.High:
                    return 10;

                case Severity.Critical:
                    return 30;
            }

            throw new NotImplementedException($"Unknown type {Severity}");
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
        Age += delta;
    }

    public float Age { get; set; }

    public bool Healed()
    {
        var roll = Random.value;
        var healed = false;
        var name = GetName();

        switch (Severity)
        {
            case Severity.Low:
                healed = Age > 5f && roll > 0.2f;
                break;
            case Severity.Medium:
                if (Age > 30f && roll > 0.5f)
                {
                    Severity = Severity.Low;
                    Owner.Log($"{name} healed to a {GetName()}");
                    Age = 0;
                }
                break;
            case Severity.High:
                if (Age > 60f && roll > 0.8f)
                {
                    Severity = Severity.Medium;
                    Owner.Log($"{name} healed to a {GetName()}");
                    Age = 0;
                }
                break;

            case Severity.Critical:
                if (Age > 120f && roll > 0.9f)
                {
                    Severity = Severity.High;
                    Owner.Log($"{name} healed to a {GetName()}");
                    Age = 0;
                }
                break;
        }

        return healed;
    }

    public Wound(Limb limb, string source, DamageType damageType, Severity severity)
    {
        Limb = limb;
        Source = source;
        DamageType = damageType;
        Severity = severity;
    }
}