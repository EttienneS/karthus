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

        // AddSubTask(new ClearCell(Coordinates));

        foreach (var requiredItem in structure.Require)
        {
            AddSubTask(new MoveItemToCell(requiredItem, Coordinates, true, true, GetItem.SearchBy.Name));
        }

        AddSubTask(new Wait(3f, "Building"));
        AddSubTask(new Move(Game.MapGrid.GetPathableNeighbour(Coordinates)));

        Message = $"Building {structure.Name} at {coordinates}";
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            foreach (var item in Creature.Mind[Context][MemoryType.Item])
            {
                Game.ItemController.DestroyItem(IdService.GetItemFromId(item));
            }

            Structure.SetBlueprintState(false);
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

        Taskmaster.ProcessQueue(SubTasks);
    }
}