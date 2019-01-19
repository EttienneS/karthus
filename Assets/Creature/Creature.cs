using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public Cell CurrentCell;
    public float Speed = 5f;

    public ITask Task;

    internal SpriteAnimator SpriteAnimator;

    public string TaskName;

    public Item CarriedItem;

    public void Start()
    {
        SpriteAnimator = GetComponent<SpriteAnimator>();
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

        if (!Task.Done())
        {
            Task.Update();
        }
        else
        {
            Taskmaster.Instance.TaskComplete(Task);
            Task = null;
        }

        if (CarriedItem != null)
        {
            CarriedItem.transform.position = transform.position;
            CarriedItem.SpriteRenderer.sortingLayerName = "CarriedItem";
        }
    }
}