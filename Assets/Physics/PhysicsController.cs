using System.Collections.Generic;
using UnityEngine;

public class PhysicsController : MonoBehaviour
{
    internal Queue<Cell> VolatileCells = new Queue<Cell>();

    public int UpdatesPerFrame = 200;

    [Range(0f, 0.2f)] public float PhysicsRate = 0.05f;
    internal float WorkTick;

    // Update is called once per frame
    private void Update()
    {
        if (!Game.Ready)
            return;
        if (Game.TimeManager.Paused)
            return;

        WorkTick += Time.deltaTime;
        if (WorkTick < PhysicsRate)
        {
            return;
        }

        WorkTick = 0;

        if (VolatileCells.Count == 0)
        {
            return;
        }

        var batch = new List<Cell>();
        for (int i = 0; i < UpdatesPerFrame; i++)
        {
            if (VolatileCells.Count == 0 || VolatileCells.Peek() == null)
            {
                break;
            }
            batch.Add(VolatileCells.Dequeue());
        }

        foreach (var cell in batch)
        {
            cell.UpdatePhysics();
        }
    }

    internal void Track(Cell cell)
    {
        if (!VolatileCells.Contains(cell))
        {
            VolatileCells.Enqueue(cell);
        }
    }
}