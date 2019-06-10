public class Harvest : TaskBase
{
    public StructureData Target;

    public Harvest()
    {
    }

    public Harvest(StructureData structure)
    {
        Target = structure;

        AddSubTask(new Move(Game.MapGrid.GetPathableNeighbour(Target.Coordinates)));
        AddSubTask(new Wait(2f, "Harvesting"));

        Message = $"Harvesting {structure.Name} at {structure.Coordinates}";
    }

    public override bool Done()
    {
        if (Faction.QueueComplete(SubTasks))
        {
            //Target.SpawnYield(Game.MapGrid.GetCellAtCoordinate(Target.Coordinates));
            Game.StructureController.DestroyStructure(Target);
            return true;
        }

        return false;
    }
}