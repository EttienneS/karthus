using UnityEngine;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    public RectTransform barFillRectTransform;
    public Text LoadingTextBox;

    private bool _loading;
    private AsyncOperation _loadingOperation;
    private Vector3 barFillLocalScale = Vector3.one;

    public void Hide()
    {
        // Disable the loading screen:
        gameObject.SetActive(false);
        _loadingOperation = null;
        _loading = false;
    }

    public void Show(AsyncOperation loadingOperation)
    {
        // Enable the loading screen:
        gameObject.SetActive(true);
        // Store the reference:
        _loadingOperation = loadingOperation;
        // Reset the UI:
        SetProgress(0f);
        _loading = true;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        Hide();
    }

    // Updates the UI based on the progress:
    private void SetProgress(float progress)
    {
        LoadingTextBox.text = Mathf.CeilToInt(progress * 100).ToString() + "%";
        barFillLocalScale.x = progress;
        barFillRectTransform.localScale = barFillLocalScale;
    }

    private void Update()
    {
        if (_loading)
        {
            if (_loadingOperation.isDone)
            {
                Hide();
            }
            else
            {
                SetProgress(_loadingOperation.progress);
            }
        }
    }
}