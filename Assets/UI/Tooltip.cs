using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Tooltip : MonoBehaviour
    {
        public Text Title;
        public Text Content;

        public void Show(string title, string content, Vector3 position)
        {
            Title.text = title;
            Content.text = content;
            transform.position = position;

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}