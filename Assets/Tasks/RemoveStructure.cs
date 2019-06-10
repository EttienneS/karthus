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
        if (Faction.QueueComplete(SubTasks))
        {
            Game.StructureController.DestroyStructure(Structure);

            return true;
        }

        return false;
    }
}