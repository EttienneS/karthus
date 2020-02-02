using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    private void Start()
    {
        Game.MainMenuController.Toggle();
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }
}