using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public Cell CurrentCell;

    public Cell TargetCell;

    private float nextActionTime;
    public float period = 1f;

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextActionTime)
        {
            nextActionTime += period;
            if (TargetCell != null && CurrentCell != TargetCell)
            {
                MoveToCell(Pathfinder.FindPath(CurrentCell, TargetCell)[1]);
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
    }
}
