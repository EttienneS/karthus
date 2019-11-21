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

        var cellToBind = _affectAbleCells.Find(c => c.BiomeId == 0);
        if (cellToBind != null)
        {
            cellToBind.BiomeId = AssignedEntity.Cell.BiomeId;
            Game.VisualEffectController.SpawnLightEffect(AssignedEntity, cellToBind.Vector, Color.magenta, 1 + Random.value * 2, 1 + Random.value * 2, 3);

            cellToBind.UpdateTile();
        }

        return true;
    }
}