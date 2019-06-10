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

        foreach (var manaCost in structure.ManaCost)
        {
            AddSubTask(new Channel(manaCost.Key, manaCost.Value));
        }

        AddSubTask(new Move(Game.MapGrid.GetPathableNeighbour(Coordinates)));
        AddSubTask(new Burn(structure.ManaCost));

        Message = $"Building {structure.Name} at {coordinates}";
    }

    public override bool Done()
    {
        if (Faction.QueueComplete(SubTasks))
        {
            Structure.SetBlueprintState(false);
            Structure.Faction = Creature.Faction;
            Creature.UpdateMemory(Context, MemoryType.Structure, Structure.GetGameId());
            return true;
        }
        return false;
    }

    public override void Update()
    {
        if (Structure == null)
        {
            throw new TaskFailedException();
        }

        Faction.ProcessQueue(SubTasks);
    }
}