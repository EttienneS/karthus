using System;
using UnityEngine;
using UnityEngine.UI;

public class ImageButton : MonoBehaviour
{
    internal Button Button;
    internal Image Image;

    public void SetImage(Sprite sprite)
    {
        Image.sprite = sprite;
    }

    public void SetOnClick(Action action)
    {
        Button.onClick.AddListener(() => action.Invoke());
    }

    private void Awake()
    {
        Button = GetComponent<Button>();
        Image = transform.Find("Image").GetComponent<Image>();
    }
}