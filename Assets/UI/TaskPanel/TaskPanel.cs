using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.UI.TaskPanel
{
    public class TaskPanel : MonoBehaviour
    {
        public TaskDisplay TaskDisplayPrefab;
        public Dictionary<CreatureTask, TaskDisplay> Tasks = new Dictionary<CreatureTask, TaskDisplay>();

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            var tasks = Game.FactionController.PlayerFaction.AssignedTasks.Keys.ToList();
            tasks.AddRange(Game.FactionController.PlayerFaction.AvailableTasks);

            foreach (var task in tasks)
            {
                if (!Tasks.ContainsKey(task))
                {
                    var taskDisplay = Instantiate(TaskDisplayPrefab, transform);
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

        public void Toggle()
        {
            if (gameObject.activeInHierarchy)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
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