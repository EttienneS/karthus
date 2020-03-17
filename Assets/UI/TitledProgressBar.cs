using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class TitledProgressBar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform barFillRectTransform;
        public Image BarImage;
        public Text Title;

        public string TooltipTitle;
        public string TooltipText;

        public void Load(string title, float start, string tooltipTitle, string tooltipText)
        {
            SetProgress(start);

            Title.text = title;
            TooltipTitle = tooltipTitle;
            TooltipText = tooltipText;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(TooltipTitle))
            {
                Game.Instance.Tooltip.Show(TooltipTitle, TooltipText);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Game.Instance.Tooltip.Hide();
        }

        public void SetProgress(float progress)
        {
            barFillRectTransform.localScale = new Vector3(progress, 1, 1);
        }
    }
}