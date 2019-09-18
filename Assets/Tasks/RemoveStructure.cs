public class RemoveStructure : EntityTask
{
    public Structure Structure;

    public RemoveStructure()
    {
    }

    public RemoveStructure(Structure structure)
    {
        Structure = structure;

        AddSubTask(new Move(Game.Map.GetPathableNeighbour(Structure.Cell)));

        foreach (var mana in structure.ManaPool)
        {
            if (mana.Value.Total > 0)
            {
                AddSubTask(Channel.GetChannelFrom(mana.Key, mana.Value.Total, structure));
            }
        }

        Message = $"Removing {Structure.Name} at {Structure.Cell}";
    }

    public override bool Done()
    {
        if (SubTasksComplete())
        {
            Game.StructureController.DestroyStructure(Structure);

            return true;
        }

        return false;
    }
}