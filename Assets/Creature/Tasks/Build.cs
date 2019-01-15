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


        var item = MapGrid.Instance.FindClosestItem(Cell);
        item.Reserved = true;

        SubTasks.Enqueue(new GetItem(item));
        SubTasks.Enqueue(new Move(Cell));
        SubTasks.Enqueue(new PlaceHeldItemInStructure(Structure));
        SubTasks.Enqueue(new Wait(1f));
        SubTasks.Enqueue(new Move(Cell.Neighbors.First(c => c.TravelCost != 0)));
    }

    public string TaskId { get; set; }
    public Queue<ITask> SubTasks { get; set; }

    public bool Done()
    {
        if (SubTasks != null && Taskmaster.QueueComplete(SubTasks))
        {
            foreach (var item in Structure.ContainedItems)
            {
                ItemController.Instance.DestoyItem(item);
            }

            Structure.BluePrint = false;

            return true;
        }
        return false;
    }

    public override string ToString()
    {
        return $"Building to {Structure.name} at {Cell.Coordinates.ToString()}";
    }

    public void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}