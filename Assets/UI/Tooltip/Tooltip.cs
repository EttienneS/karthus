using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Tooltip : MonoBehaviour
    {
        public Text Title;
        public Text Content;

        public void Load(string title, string content)
        {
            Title.text = title;
            Content.text = content;
            gameObject.SetActive(true);
            UpdatePosition();
        }

        public void Update()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            transform.position = Input.mousePosition;
            var width = 200;
            var height = 80;

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

        internal void Destroy()
        {
            Destroy(gameObject);
        }
    }
}