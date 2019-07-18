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

        foreach (var mana in Target.ManaValue)
        {
            AddSubTask(Channel.GetChannelFrom(mana.Key, mana.Value, Target.GetGameId()));
        }

        Message = $"Harvesting {structure.Name} at {structure.Coordinates}";
    }

    public override bool Done()
    {
        if (Faction.QueueComplete(SubTasks))
        {
            Game.StructureController.DestroyStructure(Target);
            return true;
        }

        return false;
    }
}