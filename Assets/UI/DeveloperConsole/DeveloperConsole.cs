using Arg;
using Assets.Creature;
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

    public IEntity GetEntity(string id)
    {
        var entity = id.GetEntity();
        if (entity == null)
        {
            entity = Game.Instance.IdService.CreatureLookup.Values.ToList().Find(c => c.Name.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
        return entity;
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
                foreach (var item in Game.Instance.IdService.ItemIdLookup)
                {
                    list += $"{item.Key}: {item.Value.Name}\n";
                }
                break;

            case "creatures":
                foreach (var creature in Game.Instance.IdService.CreatureIdLookup)
                {
                    list += $"{creature.Key}: {creature.Value.Name}\n";
                }
                break;

            case "structures":
                foreach (var structure in Game.Instance.IdService.StructureIdLookup)
                {
                    list += $"{structure.Key}: {structure.Value.Name}\n";
                }
                break;

            case "factions":
                foreach (var faction in Game.Instance.FactionController.Factions)
                {
                    list += $"{faction.Key}\n";
                }
                break;

            case "zones":
                foreach (var zone in Game.Instance.ZoneController.Zones)
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
        Game.Instance.TimeManager.Pause();
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
            Game.Instance.TimeManager.Data.Hour = int.Parse(split[0]);
            Game.Instance.TimeManager.Data.Minute = int.Parse(split[1]);
            return $"Time Set To: {Game.Instance.TimeManager.Data.Hour}:{Game.Instance.TimeManager.Data.Minute}";
        });
        Commands.Add("Load", (args) =>
        {
            SaveManager.Load(args);
            return "Loading...";
        });

        Commands.Add("Creatures", (_) => List("Creatures"));
        Commands.Add("Items", (_) => List("Items"));
        Commands.Add("Structures", (_) => List("Structures"));
        Commands.Add("Zones", (_) => List("Zones"));
        Commands.Add("Factions", (_) => List("Factions"));

        Commands.Add("CompleteStructures", (args) =>
        {
            var ids = "";
            foreach (var build in Game.Instance.FactionController
                                                   .PlayerFaction
                                                   .AvailableTasks.OfType<Build>().ToList())
            {
                build.FinishStructure();
                ids += $"{build.Blueprint.StructureName},";

                Game.Instance.FactionController.PlayerFaction.AvailableTasks.Remove(build);
            }
            return ids.Trim(',');
        });

        Commands.Add("List", List);
        Commands.Add("Inspect", (args) => GetEntity(args).ToString());
        Commands.Add("Move", (args) =>
        {
            var parts = args.Split(' ');
            var entity = GetEntity(parts[0]);

            var cell = Map.Instance.GetCellAtCoordinate(float.Parse(parts[1]), float.Parse(parts[2]));
            if (entity is CreatureData creature)
            {
                creature.X = cell.X;
                creature.Z = cell.Z;
                creature.CreatureRenderer.UpdatePosition();
            }
            else
            {
                entity.Cell = cell;
            }

            return $"Move {entity.Name} to {entity.Cell}";
        });
        Commands.Add("Set", (args) =>
        {
            var parts = args.Split(' ');
            var entity = GetEntity(parts[0]) as CreatureData;

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