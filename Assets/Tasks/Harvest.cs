public class Harvest : TaskBase
{
    public StructureData Target;

    public Harvest()
    {
    }

    public Harvest(StructureData structure)
    {
        Target = structure;

        AddSubTask(new Move(MapGrid.Instance.GetPathableNeighbour(Target.Coordinates)));
        AddSubTask(new Wait(2f, "Harvesting"));

        Message = $"Harvesting {structure.Name} at {structure.Coordinates}";
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
}