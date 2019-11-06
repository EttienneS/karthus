public class RemoveStructure : CreatureTask
{
    public Structure StructureToRemove;

    public RemoveStructure()
    {
    }

    public RemoveStructure(Structure structure)
    {
        StructureToRemove = structure;

        AddSubTask(new Move(Game.Map.GetPathableNeighbour(StructureToRemove.Cell)));

        foreach (var mana in structure.ManaPool)
        {
            if (mana.Value.Total > 0)
            {
                AddSubTask(Channel.GetChannelFrom(mana.Key, mana.Value.Total, structure));
            }
        }

        Message = $"Removing {StructureToRemove.Name} at {StructureToRemove.Cell}";
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            Game.StructureController.DestroyStructure(StructureToRemove);

            return true;
        }

        return false;
    }
}