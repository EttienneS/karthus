using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bind : TaskBase
{
    public const float BindTime = 1f;

    [JsonIgnore]
    public CellData _epicentreCell;

    public float Power;
    public float PowerRate;
    public int Size;
    private static Color charge = Color.cyan;

    private static Color fire = Color.red;

    private static Color full = Color.green;

    private static Color neutral = new Color(0.3f, 0.3f, 0.3f);

    [JsonIgnore]
    private List<CellData> _affectAbleCells;

    [JsonIgnore]
    private Coordinates _epicenter;

    public Bind()
    {
    }

    public Bind(int size, float initialPower, float powerRate)
    {
        Size = size;
        PowerRate = powerRate;
        Power = initialPower;
    }

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
                _epicentreCell = MapGrid.Instance.GetCellAtCoordinate(Epicentre);
            }
            return _epicentreCell;
        }
    }

    public override bool Done()
    {
        if (_affectAbleCells == null)
        {
            _affectAbleCells = MapGrid.Instance.GetCircle(Epicentre, Size).OrderBy(c => c.Coordinates.DistanceTo(Epicentre)).ToList();
        }

        if (Taskmaster.QueueComplete(SubTasks))
        {
            if (Power > 1)
            {
                CellData cell;
                if (EpicentreCell.Binding != Originator)
                {
                    cell = EpicentreCell;
                }
                else
                {
                    cell = _affectAbleCells.Find(c => !c.Bound);
                }

                if (cell != null)
                {
                    MapGrid.Instance.BindCell(cell, Originator);
                    Power--;
                    AddSubTask(new Pulse(Originator, neutral, fire, Random.Range(0.1f, 0.5f), Random.Range(0.1f, 0.3f)));
                }
                else
                {
                    if (Power < 10)
                    {
                        AddSubTask(new Pulse(Originator, neutral, charge, BindTime / PowerRate, 0.3f));
                        Power += BindTime;
                    }
                    else
                    {
                        AddSubTask(new Pulse(Originator, neutral, full, 1f, 1f));
                    }
                }
            }
            else
            {
                AddSubTask(new Pulse(Originator, neutral, charge, BindTime / PowerRate, 0.3f));
                Power += BindTime;
            }
        }

        return false;
    }
}