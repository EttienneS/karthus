using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bind : EffectBase
{
    public int Size;
    [JsonIgnore]
    private List<Cell> _affectAbleCells;

    public Bind()
    {
    }

    public Bind(int size)
    {
        Size = size;
    }

    public new int Range = -1;

    public override bool DoEffect()
    {
        if (_affectAbleCells == null)
        {
            _affectAbleCells = Game.Map.GetCircle(AssignedEntity.Cell, Size)
                                       .OrderBy(c => c.DistanceTo(AssignedEntity.Cell))
                                       .ToList();
        }

        var cellToBind = _affectAbleCells.Find(c => !c.Bound);
        if (cellToBind != null)
        {
            Game.Map.BindCell(cellToBind, AssignedEntity);

            Game.VisualEffectController.SpawnLightEffect(cellToBind, Color.magenta, 2, 4, 5)
                                       .Fades();
        }
        return true;
    }
}