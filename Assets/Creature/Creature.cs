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

    public void See()
    {
        foreach (var c in MapGrid.Instance.GetCircle(CurrentCell, 5))
        {
            c.Fog.enabled = false;
        }
    }

    public void Start()
    {
        SpriteAnimator = GetComponent<SpriteAnimator>();
    }

    public void Update()
    {
        if (Task == null)
        {
            Task = Taskmaster.Instance.GetTask(this);
            Task.Start(this);
        }

        TaskName = Task.ToString();

        if (!Task.Done(this))
        {
            Task.Update(this);
        }
        else
        {
            Taskmaster.Instance.TaskComplete(Task);
            Task = null;
        }
    }
    
}