public class Build : Task
{
    public Structure Structure;

    public Build()
    {
    }

    public Build(Structure structure)
    {
        Structure = structure;

        AddSubTask(new Acrue(structure.ManaValue));
        foreach (var mana in structure.ManaValue)
        {
            AddSubTask(Channel.GetChannelTo(mana.Key, mana.Value, structure));
        }

        Message = $"Building {structure.Name} at {structure.Cell}";
    }

    public override bool Done()
    {
        if (Structure == null)
        {
            throw new TaskFailedException();
        }

        if (Creature.TaskQueueComplete(SubTasks))
        {
            Structure.SetBluePrintState(false);

            if (Structure.IsWall())
            {
                foreach (var neighbour in Structure.Cell.Neighbors)
                {
                    if (neighbour != null)
                    {
                        Game.Map.RefreshCell(neighbour);
                    }
                }
            }

            Creature.GetFaction().AddStructure(Structure);
            Creature.UpdateMemory(Context, MemoryType.Structure, Structure.Id);

            if (Structure.Spell != null)
            {
                Game.MagicController.AddRune(Structure);
            }

            return true;
        }
        return false;
    }
}