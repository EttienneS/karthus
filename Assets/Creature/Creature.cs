using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Creature : MonoBehaviour
{
    public ITask Task;
    internal SpriteAnimator SpriteAnimator;
    internal SpriteRenderer SpriteRenderer;

    internal CreatureData Data = new CreatureData();

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

    public void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        SpriteAnimator = GetComponent<SpriteAnimator>();

       Data.Hunger = Random.Range(0, 15);
       Data.Thirst = Random.Range(0, 15);
        Data. Energy = Random.Range(80, 100);
    }

    public void Update()
    {
        if (TimeManager.Instance.Paused) return;

        Data.InternalTick += Time.deltaTime;

        if (Data.InternalTick >= TimeManager.Instance.TickInterval)
        {
            Data.InternalTick = 0;

            Data.Hunger += Random.value;
            Data.Thirst += Random.value;
            Data.Energy -= Random.value;
        }

        if (Task == null)
        {
            var task = Taskmaster.Instance.GetTask(this);
            AssignTask(task);

            Task = task;
        }

        Data.TaskName = Task.ToString();

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

            if (Data.CarriedItem != null)
            {
                Data.CarriedItem.Reserved = false;
                Data.CarriedItem = null;
            }

            Task = Taskmaster.Instance.GetTask(this);
            AssignTask(Task);
        }

        if (Data.CarriedItem != null)
        {
            var item = ItemController.Instance.ItemDataLookup[Data.CarriedItem];

            item.transform.position = transform.position;
            item.SpriteRenderer.sortingLayerName = "CarriedItem";
        }
    }
}

[Serializable]
public class CreatureData
{
    [SerializeField]
    public ItemData CarriedItem;

    [SerializeField]
    public CellData CurrentCell;

    [SerializeField]
    public float Energy;

    [SerializeField]
    public float Hunger;

    [SerializeField]
    public float Speed = 5f;

    [SerializeField]
    public string TaskName;

    [SerializeField]
    public float Thirst;

    [SerializeField]
    internal float InternalTick;
}