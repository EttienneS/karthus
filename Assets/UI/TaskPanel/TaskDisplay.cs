using Assets.ServiceLocator;
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
                Loc.GetStructureController().DestroyBlueprint(build.Blueprint);
            }

            if (Loc.GetFactionController().PlayerFaction.AssignedTasks.ContainsKey(_task))
            {
                Loc.GetFactionController().PlayerFaction.AssignedTasks[_task].CancelTask();
            }
            else
            {
                Loc.GetFactionController().PlayerFaction.AvailableTasks.Remove(_task);
            }
            _task.Destroy();
        }

        public void Load(CreatureTask task)
        {
            _task = task;
        }

        public void MoveUp()
        {
            if (Loc.GetFactionController().PlayerFaction.AvailableTasks.Contains(_task))
            {
                Loc.GetFactionController().PlayerFaction.AvailableTasks.Remove(_task);
                Loc.GetFactionController().PlayerFaction.AvailableTasks.Insert(0, _task);

                Loc.GetGameController().UIController.ReloadTaskPanel();
            }
        }

        public void Suspend()
        {
            if (Loc.GetFactionController().PlayerFaction.AvailableTasks.Contains(_task))
            {
                _task.Suspend(false);
                Loc.GetFactionController().PlayerFaction.AvailableTasks.Remove(_task);
                Loc.GetFactionController().PlayerFaction.AvailableTasks.Add(_task);

                Loc.GetGameController().UIController.ReloadTaskPanel();
            }
        }

        public void Update()
        {
            Title.text = _task.Message;

            if (Loc.GetFactionController().PlayerFaction.AssignedTasks.ContainsKey(_task))
            {
                CreatureIcon.Creature = Loc.GetFactionController().PlayerFaction.AssignedTasks[_task];
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