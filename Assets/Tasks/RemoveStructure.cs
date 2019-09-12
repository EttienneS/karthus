public class RemoveStructure : Task
{
    public Cell Coordinates;
    public Structure Structure;

    public RemoveStructure()
    {
    }

    public RemoveStructure(Structure structure, Cell coordinates)
    {
        Structure = structure;
        Coordinates = coordinates;

        AddSubTask(new Move(Coordinates));
        AddSubTask(new Wait(2f, "Removing"));

        Message = $"Removing {Structure.Name} at {coordinates}";
    }

    public override bool Done()
    {
        if (Creature.TaskQueueComplete(SubTasks))
        {
            Game.StructureController.DestroyStructure(Structure);

            return true;
        }

        return false;
    }
}