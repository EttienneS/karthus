using UnityEngine;

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
        Game.Instance.LoadPanel.Show();
    }

    public void Save()
    {
        SaveManager.Save();
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