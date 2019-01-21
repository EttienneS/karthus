using System.Collections.Generic;

public enum TaskStatus
{
    Available, InProgress
}

public interface ITask
{
    Queue<ITask> SubTasks { get; set; } 

    Creature Creature { get; set; }

    string TaskId { get; set; }

    bool Done();

    void Update();
}