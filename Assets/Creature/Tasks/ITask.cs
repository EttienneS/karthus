public interface ITask
{
    string TaskId { get; set; }

    void Start(Creature creature);

    void Update(Creature creature);

    bool Done(Creature creature);
}

public enum TaskStatus
{
    Available, InProgress
}
