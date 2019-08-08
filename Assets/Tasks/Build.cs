public class Build : TaskBase
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

        Message = $"Building {structure.Name} at {structure.Coordinates}";
    }

    public override bool Done()
    {
        if (Structure == null)
        {
            throw new TaskFailedException();
        }

        if (Faction.QueueComplete(SubTasks))
        {
            Structure.SetBluePrintState(false);
            
            Creature.Faction.AddStructure(Structure);
            Creature.UpdateMemory(Context, MemoryType.Structure, Structure.Id);
            return true;
        }
        return false;
    }
}