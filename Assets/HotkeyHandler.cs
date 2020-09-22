using UnityEngine;
using Assets.ServiceLocator;

namespace Assets
{
    public static class HotkeyHandler
    {
        private static TimeStep _oldTimeStep = TimeStep.Normal;

        public static void HandleHotkeys()
        {
            if (Loc.GetGameController().Typing)
            {              
                return;
            }

            if (Input.GetKeyDown("`"))
            {
                DeveloperConsole.Instance.Show();
            }
            else if (Input.GetKeyDown("space"))
            {
                if (Loc.GetTimeManager().GetTimeStep() == TimeStep.Paused)
                {
                    Loc.GetTimeManager().SetTimeStep(_oldTimeStep);
                }
                else
                {
                    _oldTimeStep = Loc.GetTimeManager().GetTimeStep();
                    Loc.GetTimeManager().Pause();
                }
            }
            else if (Input.GetKeyDown("escape"))
            {
                Loc.GetGameController().MainMenuController.Toggle();
            }
            else if (Input.GetKeyDown("1"))
            {
                Loc.GetTimeManager().SetTimeStep(TimeStep.Slow);
            }
            else if (Input.GetKeyDown("2"))
            {
                Loc.GetTimeManager().SetTimeStep(TimeStep.Normal);
            }
            else if (Input.GetKeyDown("3"))
            {
                Loc.GetTimeManager().SetTimeStep(TimeStep.Fast);
            }
            else if (Input.GetKeyDown("4"))
            {
                Loc.GetTimeManager().SetTimeStep(TimeStep.Hyper);
            }
            else if (Input.GetKeyDown("b"))
            {
                Loc.GetGameController().OrderSelectionController.BuildTypeClicked();
            }
            else if (Input.GetKeyDown("n"))
            {
                Loc.GetGameController().OrderSelectionController.DesignateTypeClicked();
            }
            else if (Input.GetKeyDown("z"))
            {
                Loc.GetGameController().OrderSelectionController.ZoneTypeClicked();
            }
            else if (Input.GetKeyDown("c"))
            {
                Loc.GetGameController().OrderSelectionController.ConstructTypeClicked();
            }
            else if (Input.GetKeyDown("e"))
            {
                Loc.Current.Get<CursorController>().RotateRight();
            }
            else if (Input.GetKeyDown("q"))
            {
                Loc.Current.Get<CursorController>().RotateLeft();
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                Loc.GetGameController().UIController.Toggle();
            }
        }
    }
}