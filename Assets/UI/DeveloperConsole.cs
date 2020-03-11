using Arg;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeveloperConsole : MonoBehaviour
{
    public Dictionary<string, Execute> Commands = new Dictionary<string, Execute>();
    public TMP_InputField InputField;
    public TMP_Text OutputField;

    internal ArgsParser Parser;

    public delegate string Execute(string args);

    public string Expand()
    {
        var expansions = string.Empty;
        foreach (var chunk in Game.Map.Chunks.ToList())
        {
            Game.Map.ExpandChunksAround(chunk.Value.Cells[0]);

            expansions += $"Expanding {chunk.Key.x}:{chunk.Key.y}\n";
        }

        return expansions;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        InputField.ActivateInputField();
        Game.Instance.Typing = false;
    }

    public string List(string type)
    {
        var list = "";
        switch (type.ToLower())
        {
            case "items":
                foreach (var item in Game.IdService.ItemIdLookup)
                {
                    list += $"{item.Key}: {item.Value.Name}\n";
                }
                break;

            case "creatures":
                foreach (var creature in Game.IdService.CreatureIdLookup)
                {
                    list += $"{creature.Key}: {creature.Value.Name}\n";
                }
                break;

            case "structures":
                foreach (var structure in Game.IdService.StructureIdLookup)
                {
                    list += $"{structure.Key}: {structure.Value.Name}\n";
                }
                break;

            case "factions":
                foreach (var faction in Game.FactionController.Factions)
                {
                    list += $"{faction.Key}\n";
                }
                break;

            case "zones":
                foreach (var zone in Game.ZoneController.Zones)
                {
                    list += $"{zone.Key.Name}: {zone.Key.FactionName}\n";
                }
                break;
        }
        return list;
    }

    public void ProcessCommand()
    {
        Debug.Log($"Process command: {InputField.text}");

        var input = "-" + InputField.text.TrimStart(new[] { '-', '/' });

        if (input.Contains(" "))
        {
            input = input.Insert(InputField.text.IndexOf(" ") + 1, ":'") + "'";
        }

        foreach (var command in Parser.Parse(input))
        {
            OutputField.text = Commands[command.Name].Invoke(command.Value.Trim());
        }
        //Hide();
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
        Commands.Add("Log", (args) =>
        {
            Debug.Log($"{args}");
            return args;
        });
        Commands.Add("SetTime", (args) =>
        {
            var split = args.Split(':');
            Game.TimeManager.Data.Hour = int.Parse(split[0]);
            Game.TimeManager.Data.Minute = int.Parse(split[1]);
            return $"Time Set To: {Game.TimeManager.Data.Hour}:{Game.TimeManager.Data.Minute}";
        });
        Commands.Add("Load", (args) =>
        {
            SaveManager.Load(args);
            return "Loading...";
        });
        Commands.Add("Expand", (_) => Expand());

        Commands.Add("Creatures", (_) => List("Creatures"));
        Commands.Add("Items", (_) => List("Items"));
        Commands.Add("Structures", (_) => List("Structures"));
        Commands.Add("Zones", (_) => List("Zones"));
        Commands.Add("Factions", (_) => List("Factions"));

        Commands.Add("List", List);
        Commands.Add("Inspect", (args) => args.GetEntity().ToString());
        Commands.Add("Set", (args) =>
        {
            var parts = args.Split(' ');
            var entity = parts[0].GetCreature();
            if (entity == null)
            {
                entity = Game.IdService.CreatureLookup.Values.ToList().Find(c => c.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase));
            }

            var need = entity.Needs.Find(n => n.Name.Equals(parts[1], StringComparison.OrdinalIgnoreCase));

            var msg = $"Changed ({entity.Id}){entity.Name}'s {need.Name} from '{need.Current}' to '{parts[2]}'";
            need.Current = int.Parse(parts[2]);

            return msg;
        });

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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }
}