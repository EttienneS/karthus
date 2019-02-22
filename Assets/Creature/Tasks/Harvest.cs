using System;
using System.Collections.Generic;

[Serializable]
public class Harvest : TaskBase
{
    public StructureData Target;

    public Harvest()
    {
    }

    public Harvest(StructureData structure)
    {
        Target = structure;

        SubTasks.Enqueue(new Move(MapGrid.Instance.GetPathableNeighbour(Target.Coordinates)));
        SubTasks.Enqueue(new Wait(2f, "Harvesting"));
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            Target.SpawnYield(MapGrid.Instance.GetCellAtCoordinate(Target.Coordinates));
            StructureController.Instance.DestroyStructure(Target);
            return true;
        }

        return false;
    }

    public override void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}