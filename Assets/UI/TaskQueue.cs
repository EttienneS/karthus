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
        var removals = Lookup.Keys.Except(Taskmaster.Instance.Tasks).ToList();
        foreach (var remove in removals)
        {
            Destroy(Lookup[remove].gameObject);
            Lookup.Remove(remove);
        }

        foreach (var task in Taskmaster.Instance.Tasks)
        {
            if (!Lookup.ContainsKey(task))
            {
                Lookup.Add(task, Instantiate(DisplayPrefab, Content.transform));
            }

            Lookup[task].SetData(task);
        }

        Scale();
    }

    private void Scale()
    {
        Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Lookup.Keys.Count * 70f);
        //ScrollRect.ScrollToTop();
    }
}