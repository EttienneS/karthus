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

        AddSubTask(new Move(Coordinates));
        AddSubTask(new Wait(2f, "Removing"));

        Message = $"Removing {Structure.Name} at {coordinates}";
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            foreach (var itemName in StructureController.Instance.StructureDataReference[Structure.Name].Require)
            {
                MapGrid.Instance.GetCellAtCoordinate(Coordinates).AddContent(ItemController.Instance.GetItem(itemName).gameObject);
            }

            StructureController.Instance.DestroyStructure(Structure);

            return true;
        }

        return false;
    }
}