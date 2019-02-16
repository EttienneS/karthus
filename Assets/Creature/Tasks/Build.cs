using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Build : TaskBase
{
    public Coordinates Coordinates;

    public StructureData Structure;

    public Build()
    {
    }

    public Build(StructureData structure, Coordinates coordinates)
    {
        Structure = structure;
        Coordinates = coordinates;


        foreach (var itemType in structure.RequiredItemTypes)
        {
            SubTasks.Enqueue(new MoveItemToCell(itemType, Coordinates, true, true));
        }

        SubTasks.Enqueue(new Wait(3f, "Building"));
        SubTasks.Enqueue(new Move(MapGrid.Instance.GetCellAtCoordinate(coordinates).Neighbors
                                    .First(c => c.TravelCost != 0).Data.Coordinates));
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            var thisCell = MapGrid.Instance.GetCellAtCoordinate(Structure.Coordinates);
            foreach (var item in thisCell.Data.ContainedItems.ToArray())
            {
                ItemController.Instance.DestroyItem(item);
            }
            Structure.SetBlueprintState(false);
            return true;
        }
        return false;
    }

    public override void Update()
    {
        if (Structure == null)
        {
            throw new CancelTaskException();
        }

        Taskmaster.ProcessQueue(SubTasks);
    }
}