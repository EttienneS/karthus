using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public Cell CurrentCell;

    public Cell TargetCell;

    private List<Cell> Path = new List<Cell>();

    public void Act()
    {
        if (TargetCell != null && CurrentCell != TargetCell && Path != null)
        {
            var nextStep = Path[Path.IndexOf(CurrentCell) - 1];

            if (nextStep.TravelCost < 0)
            {
                Pathfinder.InvalidPath(CurrentCell, TargetCell);
                Path = Pathfinder.FindPath(CurrentCell, TargetCell);
            }
            else
            {
                MoveToCell(nextStep);
            }
        }
    }

    public void MoveToCell(Cell cell)
    {
        cell.AddCreature(this);

        foreach (var c in MapGrid.Instance.GetCircle(cell, 3))
        {
            c.Fog.enabled = false;
        }
    }

    public void SetTarget(Cell cell)
    {
        TargetCell = cell;
        Path = Pathfinder.FindPath(CurrentCell, TargetCell);
    }
}
