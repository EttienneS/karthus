using System.Linq;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public Cell CurrentCell;
    public float Speed = 5f;

    public ITask Task;

    internal SpriteRenderer SpriteRenderer;
    internal SpriteAnimator SpriteAnimator;

    public string TaskName;

    public Item CarriedItem;

    public float Hunger { get; set; }
    public float Thirst { get; set; }
    public float Energy { get; set; }

    public void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        SpriteAnimator = GetComponent<SpriteAnimator>();

        Hunger = Random.Range(0, 15);
        Thirst = Random.Range(0, 15);
        Energy = Random.Range(80, 100);
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

    private float _needUpdate;

    public void Update()
    {
        if (TimeManager.Instance.Paused) return;

        _needUpdate += Time.deltaTime;

        if (_needUpdate >= TimeManager.Instance.TickInterval)
        {
            _needUpdate = 0;

            Hunger += Random.value;
            Thirst += Random.value;
            Energy -= Random.value;
        }

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