using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageButton : MonoBehaviour
{
    internal Button Button;
    internal Image Image;
    internal Image ButtonBackgroundImage;
    internal TMP_Text Text;

    public void SetText(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            Text.gameObject.SetActive(true);
            Text.text = text;
        }
        else
        {
            Text.gameObject.SetActive(false);
        }
    }

    public void SetImage(Sprite sprite)
    {
        if (sprite != null)
        {
            Image.gameObject.SetActive(true);
            Image.sprite = sprite;
        }
        else
        {
            Image.gameObject.SetActive(false);
        }
    }

    public void SetOnClick(Action action)
    {
        Button.onClick.AddListener(() => action.Invoke());
    }

    private void Awake()
    {
        Button = GetComponent<Button>();
        ButtonBackgroundImage = GetComponent<Image>();
        Image = transform.Find("Image").GetComponent<Image>();
        Text = transform.Find("Text").GetComponent<TMP_Text>();

        Text.gameObject.SetActive(false);
        Image.gameObject.SetActive(false);
    }

    internal void SetBaseColor(Color color)
    {
        ButtonBackgroundImage.color = color;
    }
}