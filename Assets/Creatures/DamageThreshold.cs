using System;
using System.Collections.Generic;
using System.Linq;

public class DamageThreshold
{
    public DamageThreshold()
    {
        BludgeoningThreshold = new SortedDictionary<float, Severity>
        {
            { 2, Severity.Low },
            { 4, Severity.Medium },
            { 6, Severity.High },
            { 9, Severity.Critical },
        };
        EnergyThreshold = new SortedDictionary<float, Severity>
        {
            { 2, Severity.Low },
            { 4, Severity.Medium },
            { 6, Severity.High },
            { 8, Severity.Critical },
        };
        PiercingThreshold = new SortedDictionary<float, Severity>
        {
            { 1, Severity.Low },
            { 2, Severity.Medium },
            { 5, Severity.High },
            { 9, Severity.Critical },
        };
        SlashingThreshold = new SortedDictionary<float, Severity>
        {
            { 1, Severity.Low },
            { 3, Severity.Medium },
            { 4, Severity.High },
            { 10, Severity.Critical },
        };
    }

    public SortedDictionary<float, Severity> BludgeoningThreshold { get; set; }
    public SortedDictionary<float, Severity> EnergyThreshold { get; set; }
    public SortedDictionary<float, Severity> PiercingThreshold { get; set; }
    public SortedDictionary<float, Severity> SlashingThreshold { get; set; }

    public (float, float) GetRangeForSeverity(DamageType type, Severity severity)
    {
        switch (type)
        {
            case DamageType.Bludgeoning:
                return GetCellTypeRange(BludgeoningThreshold, severity);

            case DamageType.Piercing:
                return GetCellTypeRange(PiercingThreshold, severity);

            case DamageType.Slashing:
                return GetCellTypeRange(SlashingThreshold, severity);

            case DamageType.Energy:
                return GetCellTypeRange(EnergyThreshold, severity);
        }

        throw new NotImplementedException();
    }

    public Severity GetSeverity(DamageType type, float value)
    {
        switch (type)
        {
            case DamageType.Bludgeoning:
                return GetSeverityForType(BludgeoningThreshold, value);

            case DamageType.Piercing:
                return GetSeverityForType(PiercingThreshold, value);

            case DamageType.Slashing:
                return GetSeverityForType(SlashingThreshold, value);

            case DamageType.Energy:
                return GetSeverityForType(EnergyThreshold, value);
        }

        throw new NotImplementedException();
    }

    public Severity GetSeverityForType(SortedDictionary<float, Severity> type, float value)
    {
        var reversedMap = type.Reverse();
        foreach (var kvp in reversedMap)
        {
            if (value > kvp.Key)
            {
                return kvp.Value;
            }
        }

        return reversedMap.Last().Value;
    }

    internal (float, float) GetCellTypeRange(SortedDictionary<float, Severity> type, Severity severity)
    {
        if (type.Count > 1)
        {
            var reversedMap = type.Reverse();
            var last = 0f;

            foreach (var kvp in reversedMap)
            {
                if (severity == kvp.Value)
                {
                    return (kvp.Key, last);
                }
                last = kvp.Key;
            }
        }
        return (0f, 1f);
    }
}