public interface ITask
{
    string TaskId { get; set; }
    void DoTask(Creature creature);
}

public enum TaskStatus
{
    Available, InProgress
}

public class MoveTask : ITask
{
    public Cell Destination { get; set; }
    public string TaskId { get; set; }

    public MoveTask(Cell destination)
    {
        Destination = destination;
        TaskId = $"Move to {Destination}";
    }

    public void DoTask(Creature creature)
    {
        creature.SetTarget(Destination);
    }
}