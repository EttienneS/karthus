public class RemoveStructure : Task
{
    public Structure Structure;

    public RemoveStructure()
    {
    }

    public RemoveStructure(Structure structure)
    {
        Structure = structure;

        AddSubTask(new Move(Game.Map.GetPathableNeighbour(Structure.Cell)));
        AddSubTask(new Wait(2f, "Removing"));

        Message = $"Removing {Structure.Name} at {Structure.Cell}";
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