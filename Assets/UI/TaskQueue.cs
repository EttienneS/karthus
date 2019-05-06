using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TaskQueue : MonoBehaviour
{
    public DataDisplay DisplayPrefab;
    public GameObject Content;
    public ScrollRect ScrollRect;
    public Dictionary<TaskBase, DataDisplay> Lookup = new Dictionary<TaskBase, DataDisplay>();

    private void Update()
    {
        foreach (var remove in Lookup.Keys.Except(Factions.Taskmasters[FactionConstants.Player].Tasks).ToList())
        {
            Destroy(Lookup[remove].gameObject);
            Lookup.Remove(remove);
        }

        foreach (var task in Factions.Taskmasters[FactionConstants.Player].Tasks)
        {
            if (!Lookup.ContainsKey(task))
            {
                Lookup.Add(task, Instantiate(DisplayPrefab, Content.transform));

                Lookup[task].Clicked += () => TaskQueue_Clicked(task);
            }

            Lookup[task].SetData(task);
        }

        Scale();
    }

    private void TaskQueue_Clicked(TaskBase task)
    {
        if (task.Failed)
        {
            task.Failed = false;
        }
        else
        {
            Factions.Taskmasters[FactionConstants.Player].TaskFailed(task, "SUSPENDED");
        }
    }

    private void Scale()
    {
        Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Lookup.Keys.Count * 70f);
        //ScrollRect.ScrollToTop();
    }
}