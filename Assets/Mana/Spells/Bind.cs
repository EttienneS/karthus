using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class Bind : EntityTask
{
    public const float BindTime = 1f;

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

    public override bool Done()
    {
        if (_affectAbleCells == null)
        {
            _affectAbleCells = Game.Map.GetCircle(AssignedEntity.Cell, Size)
                                       .OrderBy(c => c.DistanceTo(AssignedEntity.Cell))
                                       .ToList();
        }

        if (SubTasksComplete())
        {
            var cellToBind = _affectAbleCells.Find(c => !c.Bound);
            if (cellToBind != null)
            {
                Game.Map.BindCell(cellToBind, AssignedEntity);
            }
            return true;
        }

        return false;
    }
}