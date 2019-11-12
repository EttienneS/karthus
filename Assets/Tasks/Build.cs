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

    private int _waitCount = 0;

    public override bool Done(Creature creature)
    {
        if (TargetStructure == null)
        {
            throw new TaskFailedException();
        }

        if (SubTasksComplete(creature))
        {
            if (TargetStructure.Cell.Creatures.Count > 0)
            {
                _waitCount++;

                if (_waitCount > 10)
                {
                    throw new TaskFailedException("Cannot build, cell occupied");
                }
                AddSubTask(new Wait(0.5f, "Cell occupied"));
                return false;
            }

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