using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bind : BaseRune
{
    public const float BindTime = 1f;

    [JsonIgnore]
    public CellData _epicentreCell;

    public int Size;

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
                FireRune(() => MapGrid.Instance.BindCell(cell, Originator));
            }
            else
            {
                FireRune(null);
            }
        }

        return false;
    }

}