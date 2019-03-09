using System;


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

        foreach (var itemType in structure.Require)
        {
            AddSubTask(new MoveItemToCell(itemType, Coordinates, true, true));
        }

        AddSubTask(new Wait(3f, "Building"));
        AddSubTask(new Move(MapGrid.Instance.GetPathableNeighbour(Coordinates)));
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            foreach (var item in Creature.Mind[Context][MemoryType.Item])
            {
                ItemController.Instance.DestroyItem(GameIdHelper.GetItemFromId(item));
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
            throw new CancelTaskException();
        }

        Taskmaster.ProcessQueue(SubTasks);
    }
}