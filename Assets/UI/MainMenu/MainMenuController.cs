using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public bool MainMenuActive;

    public void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
        MainMenuActive = gameObject.activeInHierarchy;

        if (MainMenuActive)
        {
            Game.Instance.TimeManager.Pause();
        }
    }

    public void Restart()
    {
        SaveManager.Restart();
    }

    public void Load()
    {
        Game.Instance.ShowLoadPanel();
    }

    public void Save()
    {
        SaveManager.SaveGame();
    }

    public void ReturnToTile()
    {
        Game.Instance = null;
        SaveManager.SaveToLoad = null;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ();
#endif
    }
}