using System;
using UnityEngine;
using UnityEngine.UI;

public class ImageButton : MonoBehaviour
{
    internal Button Button;
    internal Image Image;
    internal Text Text;

    public void SetImage(Sprite sprite)
    {
        Image.sprite = sprite;
    }

    public void SetOnClick(Action action)
    {
        Button.onClick.AddListener(() => action.Invoke());
    }

    public void SetText(string text)
    {
        Text.text = text;
    }

    private void Awake()
    {
        Button = GetComponent<Button>();
        Image = transform.Find("Image").GetComponent<Image>();
        Text = GetComponentInChildren<Text>();
    }
}