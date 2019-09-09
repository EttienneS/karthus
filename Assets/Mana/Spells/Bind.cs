using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class Bind : SpellBase
{
    public const float BindTime = 1f;

    public int Size;

    [JsonIgnore]
    private List<Cell> _affectAbleCells;

    public Bind()
    {
    }

    public Bind(int size, float initialPower, float powerRate)
    {
        Size = size;
        PowerRate = powerRate;
        Power = initialPower;
    }

    public override bool Done()
    {
        if (_affectAbleCells == null)
        {
            _affectAbleCells = Game.MapGrid.GetCircle(Epicentre, Size).OrderBy(c => c.DistanceTo(Epicentre)).ToList();
        }

        if (Faction.QueueComplete(SubTasks))
        {
            Cell cell;
            if (Epicentre.Binding != Originator)
            {
                cell = Epicentre;
            }
            else
            {
                cell = _affectAbleCells.Find(c => !c.Bound);
            }

            if (cell != null)
            {
                FireRune(() => Game.MapGrid.BindCell(cell, Originator));
            }
            else
            {
                FireRune(null);
            }
        }

        return false;
    }
}