using UnityEngine;

namespace Assets.Helpers
{
    public static class InputHelper
    {
        public static bool LeftMouseButtonIsBeingClicked()
        {
            return Input.GetMouseButton(0);
        }

        public static bool LeftMouseButtonReleased()
        {
            return Input.GetMouseButtonUp(0);
        }

        public static bool LeftMouseButtonStartClick()
        {
            return Input.GetMouseButtonDown(0);
        }

        public static bool RightMouseClciked()
        {
            return Input.GetMouseButton(1);
        }
    }
}