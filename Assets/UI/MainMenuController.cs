using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public bool MainMenuActive;

    public void Start()
    {
        Game.MainMenuController.Toggle();
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
        MainMenuActive = gameObject.activeInHierarchy;

        if (MainMenuActive)
        {
            Game.TimeManager.Pause();
        }
    }

    public void Restart()
    {

    }

    public void Load()
    {
        Game.SaveManager.Load();
    }

    public void Save()
    {
        Game.SaveManager.Save();
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