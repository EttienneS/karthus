using Assets.ServiceLocator;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.UI.TaskPanel
{
    public class TaskPanel : MonoBehaviour
    {
        public TaskDisplay TaskDisplayPrefab;
        public Dictionary<CreatureTask, TaskDisplay> Tasks = new Dictionary<CreatureTask, TaskDisplay>();

        public GameObject TaskView;

        private void Update()
        {
            var tasks = Loc.GetFactionController().PlayerFaction.AssignedTasks.Keys.ToList();
            tasks.AddRange(Loc.GetFactionController().PlayerFaction.AvailableTasks);

            foreach (var task in tasks)
            {
                if (!Tasks.ContainsKey(task))
                {
                    var taskDisplay = Instantiate(TaskDisplayPrefab, TaskView.transform);
                    taskDisplay.Load(task);
                    Tasks.Add(task, taskDisplay);
                }
            }

            foreach (var task in Tasks.Keys.ToList())
            {
                if (task.Destroyed)
                {
                    Destroy(Tasks[task].gameObject);
                    Tasks.Remove(task);
                }
            }
        }

        internal void Reload()
        {
            foreach (var task in Tasks.Keys.ToList())
            {
                Destroy(Tasks[task].gameObject);
                Tasks.Remove(task);
            }
        }
    }
}