using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TaskQueue : MonoBehaviour
{
    public DataDisplay DisplayPrefab;
    public GameObject Content;
    public ScrollRect ScrollRect;
    public Dictionary<Task, DataDisplay> Lookup = new Dictionary<Task, DataDisplay>();

    private void Update()
    {
        foreach (var remove in Lookup.Keys.Except(FactionController.PlayerFaction.Tasks).ToList())
        {
            Destroy(Lookup[remove].gameObject);
            Lookup.Remove(remove);
        }

        foreach (var task in FactionController.PlayerFaction.Tasks)
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

    private void TaskQueue_Clicked(Task task)
    {
        if (task.Failed)
        {
            task.Failed = false;
        }
        else
        {
            FactionController.PlayerFaction.CancelTask(task);
        }
    }

    private void Scale()
    {
        Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Lookup.Keys.Count * 70f);
        //ScrollRect.ScrollToTop();
    }
}