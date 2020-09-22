using Assets.ServiceLocator;
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
            Loc.GetTimeManager().Pause();
        }
    }

    public void Restart()
    {
        SaveManager.Restart();
    }

    public void Load()
    {
        Loc.GetGameController().ShowLoadPanel();
    }

    public void Save()
    {
        SaveManager.SaveGame();
    }

    public void ReturnToTile()
    {
        Loc.Reset();
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