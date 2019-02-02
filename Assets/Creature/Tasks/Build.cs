using System.Collections.Generic;
using System.Linq;

public class Build : ITask
{
    public ITask _step;
    public Cell Cell;
    public Structure Structure;

    public Creature Creature { get; set; }

    public Build(Structure structure, Cell cell)
    {
        Structure = structure;
        Cell = cell;

        SubTasks = new Queue<ITask>();

        foreach (var itemType in structure.Data.RequiredItemTypes)
        {
            SubTasks.Enqueue(new MoveItemToCell(itemType, Cell, true));
            SubTasks.Enqueue(new PlaceHeldItemInStructure(Structure));
        }

        SubTasks.Enqueue(new Wait(3f, "Building"));
        SubTasks.Enqueue(new Move(Cell.Neighbors.First(c => c.TravelCost != 0)));
    }

    public string TaskId { get; set; }
    public Queue<ITask> SubTasks { get; set; }

    public bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            Structure.Data.DestroyContainedItems();
            Structure.BluePrint = false;

            if (Structure.Data.SpriteName == "Box")
            {
                var pile = Structure.gameObject.AddComponent<Stockpile>();

                if (Structure.Data.Name.Contains("Wood"))
                {
                    pile.ItemType = "Wood";
                }
                else
                {
                    pile.ItemType = "Rock";
                }
            }

            return true;
        }
        return false;
    }

    public void Update()
    {
        if (Structure == null)
        {
            throw new CancelTaskException();
        }

        Taskmaster.ProcessQueue(SubTasks);
    }
}