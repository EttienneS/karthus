using UnityEngine;

namespace Assets
{
    public static class HotkeyHandler
    {
        private static TimeStep _oldTimeStep = TimeStep.Normal;

        public static void HandleHotkeys()
        {
            if (Game.Instance.Typing)
            {              
                return;
            }

            if (Input.GetKeyDown("`"))
            {
                DeveloperConsole.Instance.Show();
            }
            else if (Input.GetKeyDown("space"))
            {
                if (Game.Instance.TimeManager.GetTimeStep() == TimeStep.Paused)
                {
                    Game.Instance.TimeManager.SetTimeStep(_oldTimeStep);
                }
                else
                {
                    _oldTimeStep = Game.Instance.TimeManager.GetTimeStep();
                    Game.Instance.TimeManager.Pause();
                }
            }
            else if (Input.GetKeyDown("escape"))
            {
                Game.Instance.MainMenuController.Toggle();
            }
            else if (Input.GetKeyDown("1"))
            {
                Game.Instance.TimeManager.SetTimeStep(TimeStep.Slow);
            }
            else if (Input.GetKeyDown("2"))
            {
                Game.Instance.TimeManager.SetTimeStep(TimeStep.Normal);
            }
            else if (Input.GetKeyDown("3"))
            {
                Game.Instance.TimeManager.SetTimeStep(TimeStep.Fast);
            }
            else if (Input.GetKeyDown("4"))
            {
                Game.Instance.TimeManager.SetTimeStep(TimeStep.Hyper);
            }
            else if (Input.GetKeyDown("b"))
            {
                Game.Instance.OrderSelectionController.BuildTypeClicked();
            }
            else if (Input.GetKeyDown("n"))
            {
                Game.Instance.OrderSelectionController.DesignateTypeClicked();
            }
            else if (Input.GetKeyDown("z"))
            {
                Game.Instance.OrderSelectionController.ZoneTypeClicked();
            }
            else if (Input.GetKeyDown("c"))
            {
                Game.Instance.OrderSelectionController.ConstructTypeClicked();
            }
            else if (Input.GetKeyDown("e"))
            {
                Game.Instance.Cursor.RotateRight();
            }
            else if (Input.GetKeyDown("q"))
            {
                Game.Instance.Cursor.RotateLeft();
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                Game.Instance.UIController.Toggle();
            }
        }
    }
}