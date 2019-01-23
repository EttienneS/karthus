using System.Linq;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public Cell CurrentCell;
    public float Speed = 5f;

    public ITask Task;

    internal SpriteRenderer SpriteRenderer;
    internal SpriteAnimator SpriteAnimator;
    internal SpriteOutline Outline;

    public string TaskName;

    public Item CarriedItem;

    public void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        SpriteAnimator = GetComponent<SpriteAnimator>();
        Outline = GetComponent<SpriteOutline>();
    }

    public void AssignTask(ITask task)
    {
        task.Creature = this;

        if (task.SubTasks != null)
        {
            foreach (var subTask in task.SubTasks.ToList())
            {
                AssignTask(subTask);
            }
        }
    }

    public void Update()
    {
        if (Task == null)
        {
            var task = Taskmaster.Instance.GetTask(this);
            AssignTask(task);

            Task = task;
        }

        TaskName = Task.ToString();

        try
        {
            if (!Task.Done())
            {
                Task.Update();
            }
            else
            {
                Taskmaster.Instance.TaskComplete(Task);
                Task = null;
            }
        }
        catch (CancelTaskException)
        {
            Taskmaster.Instance.TaskComplete(Task);

            if (CarriedItem != null)
            {
                CarriedItem.Data.Reserved = false;
                CarriedItem = null;
            }

            Task = Taskmaster.Instance.GetTask(this);
            AssignTask(Task);
        }

        if (CarriedItem != null)
        {
            CarriedItem.transform.position = transform.position;
            CarriedItem.SpriteRenderer.sortingLayerName = "CarriedItem";
        }
    }
}