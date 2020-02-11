using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeveloperConsole : MonoBehaviour
{
    public TMP_InputField InputField;

    public void Hide()
    {
        gameObject.SetActive(false);
        InputField.ActivateInputField();
        Game.Instance.Typing = false;
    }

    public void ProcessCommand()
    {
        Debug.Log($"Process command: {InputField.text}");
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Game.TimeManager.Pause();
        Game.Instance.Typing = true;

        EventSystem.current.SetSelectedGameObject(InputField.gameObject, null);
        InputField.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    public void Toggle()
    {
        if (gameObject.activeInHierarchy)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ProcessCommand();
        }
    }
}