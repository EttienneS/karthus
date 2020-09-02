using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WelcomeScreenController : MonoBehaviour
{
    public Image Background;
    public CanvasGroup Canvas;
    public Button ContinueButton;
    public GameObject DeleteOnLoad;
    public GameObject MainUiPanel;
    public string SceneToLoad;
    public Button StartButton;

    private float _delta;
    private string _lastSave;
    private Color _targetColor;

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
            ContinueButton.GetComponentInChildren<TMP_Text>().text += " - " + Path.GetDirectoryName(_lastSave).Split('\\').Last();
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
        Background.color = Color.Lerp(Background.color, _targetColor, _delta);

        if (_delta > 1f)
        {
            _targetColor = ColorExtensions.GetRandomColor();
            _delta = 0;
        }
    }

    private IEnumerator FadeLoadingScreen(float targetValue, float duration)
    {
        float startValue = Canvas.alpha;
        float time = 0;

        while (time < duration)
        {
            Canvas.alpha = Mathf.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        Canvas.alpha = targetValue;
    }

    private IEnumerator StartLoad()
    {
        MainUiPanel.SetActive(false);

        var operation = SceneManager.LoadSceneAsync(SceneToLoad, LoadSceneMode.Additive);
        while (!operation.isDone)
        {
            CycleColor();
            yield return null;
        }

        yield return StartCoroutine(FadeLoadingScreen(0, 5));

        Destroy(DeleteOnLoad);
        Destroy(gameObject);
    }
}