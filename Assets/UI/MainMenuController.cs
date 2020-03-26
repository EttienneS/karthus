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
        if (!gameObject.activeInHierarchy)
        {
            transform.localScale = new Vector3(0, 0);
            Game.TimeManager.TimeStep = TimeStep.Normal;
            gameObject.SetActive(true);
            LeanTween.scale(gameObject, new Vector3(1, 1), 0.5f).setDelay(0.3f).setOnComplete(() =>
            {
                Game.TimeManager.Pause();
                MainMenuActive = true;
            });
        }
        else
        {
            Game.TimeManager.TimeStep = TimeStep.Normal;
            LeanTween.scale(gameObject, new Vector3(0, 0), 0.5f).setOnComplete(() =>
            {
                gameObject.SetActive(false);
                MainMenuActive = false;
            });
        }
    }

    public void Restart()
    {
        SaveManager.Restart();
    }

    public void Load()
    {
        Game.LoadPanel.Show();
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