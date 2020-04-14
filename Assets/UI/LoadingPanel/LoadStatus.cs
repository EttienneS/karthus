using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadStatus : MonoBehaviour
{
    public RectTransform barFillRectTransform;
    public Image BarImage;
    public Text LoadingTextBox;
    internal List<Color> Colors = new List<Color>();
    internal Color CurrentColor;
    internal float Cycle = 3f;
    internal Color IntermColor;
    internal Color TargetColor;
    internal float TimeLeft = 3f;
    private Vector3 barFillLocalScale = Vector3.one;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Start()
    {
        Colors.Add(ColorConstants.BlueAccent);
        Colors.Add(ColorConstants.RedAccent);
        Colors.Add(ColorConstants.GreenAccent);
        Colors.Add(ColorConstants.PurpleAccent);
        Colors.Add(ColorConstants.YellowAccent);

        CurrentColor = Colors.GetRandomItem();
        TargetColor = Colors.GetRandomItem();
        Colors.Remove(CurrentColor);
        Colors.Remove(TargetColor);
    }

    public void Update()
    {
        if (!Game.Instance.Ready)
        {
            SetProgress(Game.Instance.LoadStatus, Game.Instance.LoadProgress);
        }
        else
        {
            Hide();
        }

        if (TimeLeft <= Time.deltaTime)
        {
            BarImage.color = TargetColor;

            Colors.Add(CurrentColor);
            CurrentColor = TargetColor;
            TargetColor = Colors.GetRandomItem();
            TimeLeft = Cycle;
        }
        else
        {
            IntermColor = Color.Lerp(IntermColor, TargetColor, Time.deltaTime / TimeLeft);
            BarImage.color = IntermColor;

            TimeLeft -= Time.deltaTime;
        }
    }

    private void SetProgress(string message, float progress)
    {
        LoadingTextBox.text = message;
        barFillLocalScale.x = progress;
        barFillRectTransform.localScale = barFillLocalScale;
    }
}