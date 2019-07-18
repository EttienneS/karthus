public class Build : TaskBase
{
    public Coordinates Coordinates;

    public StructureData Structure;

    public Build()
    {
    }

    public Build(StructureData structure, Coordinates coordinates)
    {
        Structure = structure;
        Coordinates = coordinates;

        AddSubTask(new Acrue(structure.ManaValue));
        foreach (var mana in structure.ManaValue)
        {
            AddSubTask(Channel.GetChannelTo(mana.Key, mana.Value, structure.GetGameId()));
        }

        Message = $"Building {structure.Name} at {coordinates}";
    }

    public override bool Done()
    {
        if (Structure == null)
        {
            throw new TaskFailedException();
        }

        if (Faction.QueueComplete(SubTasks))
        {
            Structure.SetBlueprintState(false);
            
            Creature.Faction.AddStructure(Structure);
            Creature.UpdateMemory(Context, MemoryType.Structure, Structure.GetGameId());
            return true;
        }
        return false;
    }
}