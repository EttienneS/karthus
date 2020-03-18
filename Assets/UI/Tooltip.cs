using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Tooltip : MonoBehaviour
    {
        public Text Title;
        public Text Content;

        public Vector3? Anchor;

        public void Show(string title, string content, Vector3? anchor = null)
        {
            Title.text = title;
            Content.text = content;
            Anchor = anchor;
            gameObject.SetActive(true);
        }

        public void Update()
        {
            if (Anchor.HasValue)
            {
                transform.position = Anchor.Value;
            }
            else
            {
                transform.position = Input.mousePosition;
                var width = 240;
                var height = 100;

                var offsetX = (width / 2) + 10;
                var offsetY = (height / 2) + 10;

                if (Input.mousePosition.y + height > Screen.height)
                {
                    offsetY *= -1;
                }
                if (Input.mousePosition.x + width > Screen.width)
                {
                    offsetX *= -1;
                }
                transform.position += new Vector3(offsetX, offsetY, 0);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}