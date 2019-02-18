using System.Collections.Generic;

public class RemoveStructure : TaskBase
{
    public Coordinates Coordinates;
    public StructureData Structure;

    public RemoveStructure()
    {
    }

    public RemoveStructure(StructureData structure, Coordinates coordinates)
    {
        Structure = structure;
        Coordinates = coordinates;

        SubTasks.Enqueue(new Move(Coordinates));
        SubTasks.Enqueue(new Wait(2f, "Removing"));
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            foreach (var itemName in StructureController.Instance.StructureDataReference[Structure.Name].Require)
            {
                MapGrid.Instance.GetCellAtCoordinate(Coordinates).AddContent(ItemController.Instance.GetItem(itemName).gameObject, true);
            }

            StructureController.Instance.DestroyStructure(Structure);

            return true;
        }

        return false;
    }

    public override void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}