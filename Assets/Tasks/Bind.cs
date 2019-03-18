using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bind : TaskBase
{
    public string BinderId;
    public Coordinates Epicentre;
    public int Size;
    public float PowerRate;
    public float Power;

    public const float BindTime = 1f;

    public Bind()
    {
    }

    public Bind(string binderId, Coordinates epicentre, int size, float initialPower, float powerRate)
    {
        BinderId = binderId;
        Epicentre = epicentre;
        Size = size;
        PowerRate = powerRate;
        Power = initialPower;
    }

    [JsonIgnore]
    private List<CellData> _affectAbleCells;

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
                var cell = _affectAbleCells.Find(c => !c.Bound);

                if (cell != null)
                {
                    MapGrid.Instance.BindCell(cell, BinderId);
                    Power--;
                    AddSubTask(new Pulse(BinderId, Color.white, Color.red, Random.Range(0.1f, 0.5f), Random.Range(0.1f, 0.3f)));
                }
                else
                {
                    if (Power < 10)
                    {
                        AddSubTask(new Pulse(BinderId, Color.white, Color.blue, BindTime / PowerRate, 0.3f));
                        Power += BindTime;
                    }
                    else
                    {
                        AddSubTask(new Pulse(BinderId, Color.white, Color.green, 1f, 1f));
                    }
                }
            }
            else
            {
                AddSubTask(new Pulse(BinderId, Color.white, Color.blue, BindTime / PowerRate, 0.3f));
                Power += BindTime;
            }
        }

        return false;
    }
}