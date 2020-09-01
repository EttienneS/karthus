using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WelcomeScreenController : MonoBehaviour
{
    public Image background;
    public CanvasGroup canvas;
    public GameObject mainUiPanel;
    public string sceneToLoad;

    private float _delta;
    private string _lastSave;
    private Color _targetColor;

    public Button StartButton;
    public Button ContinueButton;

    public void ContinueGame()
    {
        Game.Instance = null;
        SaveManager.SaveToLoad = Save.FromFile(_lastSave);

        StartCoroutine(StartLoad());
    }

    public void Start()
    {
        _targetColor = ColorExtensions.GetRandomColor();

        try
        {
            _lastSave = SaveManager.GetLastSave();
        }
        catch (FileNotFoundException)
        {
            ContinueButton.enabled = false;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
        StartCoroutine(StartLoad());
    }

    public void Update()
    {
        CycleColor();
    }

    private void CycleColor()
    {
        _delta += Time.deltaTime / 2f;
        background.color = Color.Lerp(background.color, _targetColor, _delta);

        if (_delta > 1f)
        {
            _targetColor = ColorExtensions.GetRandomColor();
            _delta = 0;
        }
    }

    private IEnumerator FadeLoadingScreen(float targetValue, float duration)
    {
        float startValue = canvas.alpha;
        float time = 0;

        while (time < duration)
        {
            canvas.alpha = Mathf.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        canvas.alpha = targetValue;
    }

    private IEnumerator StartLoad()
    {
        mainUiPanel.SetActive(false);

        var operation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        while (!operation.isDone)
        {
            CycleColor();
            yield return null;
        }

        yield return StartCoroutine(FadeLoadingScreen(0, 5));

        Destroy(gameObject);
    }
}