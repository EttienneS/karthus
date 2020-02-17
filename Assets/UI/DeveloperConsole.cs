using Arg;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeveloperConsole : MonoBehaviour
{
    public Dictionary<string, Execute> Commands = new Dictionary<string, Execute>();
    public TMP_InputField InputField;

    internal ArgsParser Parser;

    public delegate void Execute(string args);

    public void Expand()
    {
        foreach (var chunk in Game.Map.Chunks.ToList())
        {
            Game.Map.ExpandChunksAround(chunk.Value.Cells[0]);
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

        var input = "-" + InputField.text.TrimStart(new[] { '-', '/' });

        if (input.Contains(" "))
        {
            input = input.Insert(InputField.text.IndexOf(" "), ":'") + "'";
        }

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
        Commands.Add("Expand", (_) => Expand());

        Parser = new ArgsParser();
        foreach (var command in Commands)
        {
            Parser.ArgumentDefinitions.Add(new StringArgument(command.Key));
        }
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