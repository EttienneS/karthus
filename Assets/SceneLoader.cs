using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    public RectTransform barFillRectTransform;
    public Image BarImage;
    public Text LoadingTextBox;
    internal List<Color> Colors = new List<Color>();
    internal Color CurrentColor;
    internal float Cycle = 3f;
    internal Color IntermColor;
    internal Color TargetColor;
    internal float TimeLeft = 3f;
    private bool _done;
    private Vector3 barFillLocalScale = Vector3.one;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public IEnumerator LoadNewScene()
    {
        var async = SceneManager.LoadSceneAsync("Main", LoadSceneMode.Additive);

        while (!async.isDone)
        {
            SetProgress("Loading scene", 0.05f);
            yield return null;
        }
         
        _done = true;
    }

    public void Start()
    {
        Colors.Add(ManaColor.Blue.GetActualColor());
        Colors.Add(ManaColor.Red.GetActualColor());
        Colors.Add(ManaColor.Green.GetActualColor());
        Colors.Add(ManaColor.White.GetActualColor());
        Colors.Add(ManaColor.Black.GetActualColor());

        CurrentColor = Colors.GetRandomItem();
        TargetColor = Colors.GetRandomItem();
        Colors.Remove(CurrentColor);
        Colors.Remove(TargetColor);

        StartCoroutine(LoadNewScene());
    }

    public void Update()
    {
        if (_done)
        {
            if (!Game.Ready)
            {
                SetProgress(Game.LoadStatus, Game.LoadProgress);
            }
            else
            {
                Hide();
            }
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