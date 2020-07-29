using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.UI
{
    public class UIElementDragger : EventTrigger
    {
        private bool _dragging;
        public int OffsetY = 125;

        public void Update()
        {
            if (_dragging)
            {
                transform.parent.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y - OffsetY);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            _dragging = true;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            _dragging = false;
        }
    }
}