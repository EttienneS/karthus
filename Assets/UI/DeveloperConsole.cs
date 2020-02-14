using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Arg;

public class DeveloperConsole : MonoBehaviour
{
    public TMP_InputField InputField;

    public delegate void Execute(string args);

    public Dictionary<string, Execute> Commands = new Dictionary<string, Execute>();

    internal ArgsParser Parser;

    public void Start()
    {
        Commands.Add("Log", (args) => Debug.Log($"{args}"));
        Commands.Add("SetTime", (args) =>
        {
            var split = args.Split(':');
            Game.TimeManager.Data.Hour = int.Parse(split[0]);
            Game.TimeManager.Data.Minute = int.Parse(split[1]);
        });
        Commands.Add("Load", SaveManager.Load);

        Parser = new ArgsParser();
        foreach (var command in Commands)
        {
            Parser.ArgumentDefinitions.Add(new StringArgument(command.Key));
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        InputField.ActivateInputField();
        Game.Instance.Typing = false;
    }

    public void ProcessCommand()
    {
        Debug.Log($"Process command: {InputField.text}");

        var input = "-" + InputField.text.TrimStart(new[] { '-', '/' })
                                         .Insert(InputField.text.IndexOf(" "), ":'") + "'";

        foreach (var command in Parser.Parse(input))
        {
            Commands[command.Name].Invoke(command.Value.Trim());
        }
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
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ProcessCommand();
        }
    }
}