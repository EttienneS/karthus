using UnityEngine;

public class Creature : MonoBehaviour
{
    public Cell CurrentCell;

    public Cell TargetCell;

    public void Act()
    {
        if (TargetCell != null && CurrentCell != TargetCell)
        {
            MoveToCell(Pathfinder.FindPath(CurrentCell, TargetCell)[1]);
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
    }
}
