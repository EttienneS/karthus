using UnityEngine;

namespace Assets.Helpers
{
    public static class InputHelper
    {
        private static float _clickCounter = 0;
        private static float _clickdelay = 1f;
        private static float _lastClickTime = 0;

        public static bool LeftMouseButtonDoubleClicked()
        {
            if (LeftMouseButtonReleased())
            {
                _clickCounter++;
                if (_clickCounter == 1)
                {
                    _lastClickTime = Time.time;
                }
                else if (_clickCounter > 1 && Time.time - _lastClickTime < _clickdelay)
                {
                    _clickCounter = 0;
                    _lastClickTime = 0;
                    return true;
                }
                else if (_clickCounter > 2 || Time.time - _lastClickTime > 1)
                {
                    _clickCounter = 0;
                }
            }

            return false;
        }

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