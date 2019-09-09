using Newtonsoft.Json;
using UnityEngine;

public class SpellBase : TaskBase
{
    public const float EffectTime = 1f;

    public float Power;
    public float PowerRate;

    public static Color ChargeColor = Color.cyan;
    public static Color FullColor = Color.green;
    public static Color NeutralColor = new Color(0.3f, 0.3f, 0.3f);
    public static Color FiringColor = ColorConstants.InvalidColor;

    [JsonIgnore]
    public Structure _runeStructure;

    [JsonIgnore]
    public Structure RuneStructure
    {
        get
        {
            if (_runeStructure == null)
            {
                _runeStructure = Epicentre.Structure;
            }

            return _runeStructure;
        }
    }

    [JsonIgnore]
    public Cell _epicentreCell;

    [JsonIgnore]
    public Cell Epicentre
    {
        get
        {
            if (_epicenter == null)
            {
                _epicenter = Originator.Cell;
            }
            return _epicenter;
        }
    }

    [JsonIgnore]
    private Cell _epicenter;

    public SpellBase()
    {
    }

    public SpellBase(float initialPower, float powerRate)
    {
        PowerRate = powerRate;
        Power = initialPower;
    }

    public override bool Done()
    {
        if (Faction.QueueComplete(SubTasks))
        {
            FireRune(null);
        }

        return false;
    }

    public delegate void RuneAction();

    public void FireRune(RuneAction action)
    {
        if (Power > 1)
        {
            if (action != null)
            {
                action();
                Power--;
                AddSubTask(new Wait(1f, ""));
            }
            else
            {
                if (Power < 10)
                {
                    AddSubTask(new Wait(EffectTime / PowerRate, ""));
                    Power += EffectTime;
                }
                else
                {
                    AddSubTask(new Wait(1f, ""));
                }
            }
        }
        else
        {
            AddSubTask(new Wait(EffectTime / PowerRate, ""));
            Power += EffectTime;
        }
    }
}