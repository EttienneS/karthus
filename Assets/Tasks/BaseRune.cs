using Newtonsoft.Json;
using UnityEngine;

public class BaseRune : TaskBase
{
    public const float EffectTime = 1f;

    public float Power;
    public float PowerRate;

    public static Color ChargeColor = Color.cyan;
    public static Color FullColor = Color.green;
    public static Color NeutralColor = new Color(0.3f, 0.3f, 0.3f);
    public static Color FiringColor = ColorConstants.InvalidColor;

    [JsonIgnore]
    public StructureData _runeStructure;

    [JsonIgnore]
    public StructureData RuneStructure
    {
        get
        {
            if (_runeStructure == null)
            {
                _runeStructure = EpicentreCell.Structure;                
            }

            return _runeStructure;
        }
    }


    [JsonIgnore]
    public CellData _epicentreCell;

    [JsonIgnore]
    public Coordinates Epicentre
    {
        get
        {
            if (_epicenter == null)
            {
                _epicenter = IdService.GetLocation(Originator);
            }
            return _epicenter;
        }
    }

    [JsonIgnore]
    public CellData EpicentreCell
    {
        get
        {
            if (_epicentreCell == null)
            {
                _epicentreCell = Game.MapGrid.GetCellAtCoordinate(Epicentre);
            }
            return _epicentreCell;
        }
    }

    [JsonIgnore]
    private Coordinates _epicenter;

    public BaseRune()
    {
    }

    public BaseRune(float initialPower, float powerRate)
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
                AddSubTask(new Pulse(Originator, NeutralColor, FiringColor, Random.Range(0.1f, 0.5f), Random.Range(0.1f, 0.3f)));
            }
            else
            {
                if (Power < 10)
                {
                    AddSubTask(new Pulse(Originator, NeutralColor, ChargeColor, EffectTime / PowerRate, 0.3f));
                    Power += EffectTime;
                }
                else
                {
                    AddSubTask(new Pulse(Originator, NeutralColor, FullColor, 1f, 1f));
                }
            }
        }
        else
        {
            AddSubTask(new Pulse(Originator, NeutralColor, ChargeColor, EffectTime / PowerRate, 0.3f));
            Power += EffectTime;
        }
    }
}