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
    public TMP_InputField SeedInput;
    public Slider SizeSlider;
    public Slider SpawnSlider;
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
        _targetColor = ColorExtensions.GetRandomGray(0.8f,1f);

        try
        {
            _lastSave = SaveManager.GetLastSave();
            ContinueButton.GetComponentInChildren<TMP_Text>().text += " - " + Path.GetDirectoryName(_lastSave).Split('\\').Last();
        }
        catch (FileNotFoundException)
        {
            ContinueButton.enabled = false;
            ContinueButton.image.color = ColorConstants.GreyAccent;
        }

        SeedInput.text = NameHelper.GetRandomName() + " " + NameHelper.GetRandomName();
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
        if (string.IsNullOrEmpty(SeedInput.text))
        {
            SeedInput.text = NameHelper.GetRandomName() + " " + NameHelper.GetRandomName();
        }

        Game.MapGenerationData = new MapGenerationData(SeedInput.text)
        {
            Size = (int)SizeSlider.value,
            CreaturesToSpawn = (int)SpawnSlider.value,
        };
        StartCoroutine(StartLoad());
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ();
#endif
    }

    public void Update()
    {
        CycleColor();
    }

    private void CycleColor()
    {
        _delta += Time.deltaTime / 5f;
        Background.color = Color.Lerp(Background.color, _targetColor, _delta);

        if (_delta > 1f)
        {
            _targetColor = ColorExtensions.GetRandomGray(0.8f, 1f);
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