using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WelcomeScreenController : MonoBehaviour
{
    public CanvasGroup canvas;
    public GameObject mainUiPanel;
    public Image background;

    public string sceneToLoad;

    public void Start()
    {
        _targetColor = ColorExtensions.GetRandomColor();
        DontDestroyOnLoad(gameObject);
    }

    private Color _targetColor;
    private float _delta;

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

    public void StartGame()
    {
        StartCoroutine(StartLoad());
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