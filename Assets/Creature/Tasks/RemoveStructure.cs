using System.Collections.Generic;

public class RemoveStructure : ITask
{
    private Structure Structure;
    private Cell Cell;

    public RemoveStructure(Structure structure, Cell cell)
    {
        Structure = structure;
        Cell = cell;

        SubTasks = new Queue<ITask>();
        SubTasks.Enqueue(new Move(Cell));
        SubTasks.Enqueue(new Wait(2f));
    }

    public Queue<ITask> SubTasks { get; set; }
    public Creature Creature { get; set; }
    public string TaskId { get; set; }

    public bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            StructureController.Instance.RemoveStructure(Structure);

            foreach (var itemName in StructureController.Instance.StructureDataReference[Structure.Data.Name].RequiredItemTypes)
            {
                Cell.AddContent(ItemController.Instance.GetItem(itemName).gameObject, true);
            }

            return true;
        }

        return false;
    }

    public void Update()
    {
        Taskmaster.ProcessQueue(SubTasks);
    }
}