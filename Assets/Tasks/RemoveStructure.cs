public class RemoveStructure : CreatureTask
{
    public Structure StructureToRemove;

    public RemoveStructure()
    {
        RequiredSkill = "Build";
        RequiredSkillLevel = 1;
    }

    public RemoveStructure(Structure structure) : this()
    {
        StructureToRemove = structure;

        AddSubTask(new Move(Game.Map.GetPathableNeighbour(StructureToRemove.Cell)));

        foreach (var mana in structure.ManaValue)
        {
            if (mana.Value > 0)
            {
                AddSubTask(Channel.GetChannelFrom(mana.Key, mana.Value, structure));
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