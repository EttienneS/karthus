public class Build : EntityTask
{
    public Structure TargetStructure;

    public Build()
    {
    }

    public Build(Structure structure)
    {
        TargetStructure = structure;

        AddSubTask(new Acrue(structure.ManaValue));
        foreach (var mana in structure.ManaValue)
        {
            AddSubTask(Channel.GetChannelTo(mana.Key, mana.Value, structure));
        }

        Message = $"Building {structure.Name} at {structure.Cell}";
    }

    public override bool Done()
    {
        if (TargetStructure == null)
        {
            throw new TaskFailedException();
        }

        if (SubTasksComplete())
        {
            TargetStructure.SetBluePrintState(false);

            if (TargetStructure.IsWall())
            {
                foreach (var neighbour in TargetStructure.Cell.Neighbors)
                {
                    if (neighbour != null)
                    {
                        neighbour.UpdateTile();
                    }
                }
            }

            AssignedEntity.GetFaction().AddStructure(TargetStructure);
            CreatureData?.UpdateMemory(Context, MemoryType.Structure, TargetStructure.Id);

            if (TargetStructure.Spell != null)
            {
                Game.MagicController.AddRune(TargetStructure);
            }

            return true;
        }
        return false;
    }
}