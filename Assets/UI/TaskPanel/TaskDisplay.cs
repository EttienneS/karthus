using UnityEngine;
using UnityEngine.UI;

namespace Assets.UI.TaskPanel
{
    public class TaskDisplay : MonoBehaviour
    {
        public Image Background;
        public CreatureIcon CreatureIcon;
        public Button DeleteButton;
        public Button NowButton;
        public Button SuspendButton;
        public Text Title;
        private CreatureTask _task;

        public void Delete()
        {
            if (_task is Build build)
            {
                Game.StructureController.DestroyStructure(build.TargetStructure);
            }

            if (Game.FactionController.PlayerFaction.AssignedTasks.ContainsKey(_task))
            {
                Game.FactionController.PlayerFaction.AssignedTasks[_task].CancelTask();
            }
            else
            {
                Game.FactionController.PlayerFaction.AvailableTasks.Remove(_task);
            }
            _task.Destroy();
        }

        public void Load(CreatureTask task)
        {
            _task = task;
        }

        public void MoveUp()
        {
            if (Game.FactionController.PlayerFaction.AvailableTasks.Contains(_task))
            {
                Game.FactionController.PlayerFaction.AvailableTasks.Remove(_task);
                Game.FactionController.PlayerFaction.AvailableTasks.Insert(0, _task);

                Game.TaskPanel.Reload();
            }
        }

        public void Suspend()
        {
            if (Game.FactionController.PlayerFaction.AvailableTasks.Contains(_task))
            {
                _task.Suspend();
                Game.FactionController.PlayerFaction.AvailableTasks.Remove(_task);
                Game.FactionController.PlayerFaction.AvailableTasks.Add(_task);

                Game.TaskPanel.Reload();
            }
        }

        public void Update()
        {
            Title.text = _task.Message;

            if (Game.FactionController.PlayerFaction.AssignedTasks.ContainsKey(_task))
            {
                CreatureIcon.Creature = Game.FactionController.PlayerFaction.AssignedTasks[_task];
                NowButton.gameObject.SetActive(false);
                SuspendButton.gameObject.SetActive(false);
            }
            else
            {
                NowButton.gameObject.SetActive(true);
                SuspendButton.gameObject.SetActive(true);
            }

            if (_task.Suspended)
            {
                Background.color = ColorConstants.UISuspended;
            }
            else
            {
                Background.color = ColorConstants.UIDefault;
            }
        }
    }
}