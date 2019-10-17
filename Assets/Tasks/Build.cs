public class Build : CreatureTask
{
    public Structure TargetStructure;

    public Build()
    {
        RequiredSkill = "Build";
    }

    public Build(Structure structure) : this()
    {
        TargetStructure = structure;

        AddSubTask(new Acrue(structure.ManaValue));
        foreach (var mana in structure.ManaValue)
        {
            AddSubTask(Channel.GetChannelTo(mana.Key, mana.Value, structure));
        }

        Message = $"Building {structure.Name} at {structure.Cell}";
    }

    public override bool Done(CreatureData creature)
    {
        if (TargetStructure == null)
        {
            throw new TaskFailedException();
        }

        if (SubTasksComplete(creature))
        {
            TargetStructure.SetBluePrintState(false);

            if (TargetStructure.IsInterlocking())
            {
                foreach (var neighbour in TargetStructure.Cell.Neighbors)
                {
                    if (neighbour != null)
                    {
                        neighbour.UpdateTile();
                    }
                }
            }

            creature.GetFaction().AddStructure(TargetStructure);
            creature.UpdateMemory(Context, MemoryType.Structure, TargetStructure.Id);

            if (TargetStructure.AutoInteractions.Count > 0)
            {
                Game.MagicController.AddEffector(TargetStructure);
            }

            return true;
        }
        return false;
    }
}