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
                Game.Instance.StructureController.DestroyBlueprint(build.Blueprint);
            }

            if (Game.Instance.FactionController.PlayerFaction.AssignedTasks.ContainsKey(_task))
            {
                Game.Instance.FactionController.PlayerFaction.AssignedTasks[_task].CancelTask();
            }
            else
            {
                Game.Instance.FactionController.PlayerFaction.AvailableTasks.Remove(_task);
            }
            _task.Destroy();
        }

        public void Load(CreatureTask task)
        {
            _task = task;
        }

        public void MoveUp()
        {
            if (Game.Instance.FactionController.PlayerFaction.AvailableTasks.Contains(_task))
            {
                Game.Instance.FactionController.PlayerFaction.AvailableTasks.Remove(_task);
                Game.Instance.FactionController.PlayerFaction.AvailableTasks.Insert(0, _task);

                Game.Instance.UIController.ReloadTaskPanel();
            }
        }

        public void Suspend()
        {
            if (Game.Instance.FactionController.PlayerFaction.AvailableTasks.Contains(_task))
            {
                _task.Suspend(false);
                Game.Instance.FactionController.PlayerFaction.AvailableTasks.Remove(_task);
                Game.Instance.FactionController.PlayerFaction.AvailableTasks.Add(_task);

                Game.Instance.UIController.ReloadTaskPanel();
            }
        }

        public void Update()
        {
            Title.text = _task.Message;

            if (Game.Instance.FactionController.PlayerFaction.AssignedTasks.ContainsKey(_task))
            {
                CreatureIcon.Creature = Game.Instance.FactionController.PlayerFaction.AssignedTasks[_task];
                NowButton.enabled = false;
                NowButton.image.color = ColorConstants.GreyAccent;
                SuspendButton.enabled = false;
                SuspendButton.image.color = ColorConstants.GreyAccent;
            }
            else
            {
                NowButton.enabled = true;
                NowButton.image.color = ColorConstants.GreenAccent;
                SuspendButton.enabled = true;
                SuspendButton.image.color = ColorConstants.YellowAccent;
            }

            if (_task.IsSuspended())
            {
                Background.color = ColorConstants.YellowAccent;
                Title.text = "Suspended: " + Title.text;
            }
            else
            {
                Background.color = ColorConstants.WhiteAccent;
            }
        }
    }
}