using Arg;
using Assets.Creature;
using Assets.Map;
using Assets.Structures.Behaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeveloperConsole : MonoBehaviour
{
    public Dictionary<string, Execute> Commands = new Dictionary<string, Execute>();
    internal ArgsParser Parser;
    private static DeveloperConsole _instance;

    private string _input;

    private string _output;

    private Vector2 _scroll = Vector2.zero;
    private bool _showingConsole;

    public delegate string Execute(string args);

    public static DeveloperConsole Instance
    {
        get
        {
            return _instance != null ? _instance : (_instance = FindObjectOfType<DeveloperConsole>());
        }
        set
        {
            _instance = value;
        }
    }

    public void Hide()
    {
        Game.Instance.Typing = false;
        _showingConsole = false;
    }

    public bool IsActive()
    {
        return _showingConsole;
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

            default:
                list = "Provide type of list:\n-items\n-creatures\n-structures\n-factions\n-zones";
                break;
        }
        return list;
    }

    public void ProcessCommand()
    {
        if (string.IsNullOrEmpty(_input))
        {
            Hide();
            return;
        }

        Debug.Log($"Process command: {_input}");

        var input = "-" + _input.TrimStart(new[] { '-', '/' });

        if (input.Contains(" "))
        {
            input = input.Insert(_input.IndexOf(" ") + 1, ":'") + "'";
        }

        foreach (var command in Parser.Parse(input))
        {
            SetOutput(Commands[command.Name].Invoke(command.Value.Trim()));
        }
    }

    public void Show()
    {
        Game.Instance.TimeManager.Pause();
        Game.Instance.Typing = true;

        _showingConsole = true;
    }

    public void Start()
    {
        Commands.Add("Help", (_) => PrintHelp());
        Commands.Add("SetTime", SetTime);
        Commands.Add("Creatures", (_) => List("Creatures"));
        Commands.Add("Items", (_) => List("Items"));
        Commands.Add("Structures", (_) => List("Structures"));
        Commands.Add("Zones", (_) => List("Zones"));
        Commands.Add("Factions", (_) => List("Factions"));
        Commands.Add("Burn", (_) => Burn());
        Commands.Add("CompleteStructures", (_) => CompleteStructures());
        Commands.Add("List", List);
        Commands.Add("Move", MoveCreature);
        Commands.Add("Set", SetNeed);

        Parser = new ArgsParser();
        foreach (var command in Commands)
        {
            Parser.ArgumentDefinitions.Add(new StringArgument(command.Key));
        }
    }

    internal void OnGUI()
    {
        if (!_showingConsole)
        {
            return;
        }

        DrawDebugConsole();
    }

    private static string CompleteStructures()
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
    }

    private static CreatureData GetCreature(string nameOrId)
    {
        var creature = nameOrId.GetCreature();
        if (creature == null)
        {
            creature = Game.Instance.IdService.CreatureIdLookup.Values.ToList().Find(c => c.Name.Equals(nameOrId, StringComparison.OrdinalIgnoreCase));
        }

        return creature;
    }

    private static string Load(string args)
    {
        SaveManager.Load(args);
        return "Loading...";
    }

    private static string MoveCreature(string args)
    {
        var parts = args.Split(' ');
        var creature = GetCreature(parts[0]);
        var cell = MapController.Instance.GetCellAtCoordinate(float.Parse(parts[1]), float.Parse(parts[2]));
        creature.X = cell.X;
        creature.Z = cell.Z;
        creature.CreatureRenderer.UpdatePosition();

        return $"Move {creature.Name} to {creature.Cell}";
    }

    private static string SetNeed(string args)
    {
        var parts = args.Split(' ');
        var creature = GetCreature(parts[0]);
        var need = creature.Needs.Find(n => n.Name.Equals(parts[1], StringComparison.OrdinalIgnoreCase));
        var msg = $"Changed ({creature.Id}){creature.Name}'s {need.Name} from '{need.Current}' to '{parts[2]}'";
        need.Current = int.Parse(parts[2]);

        return msg;
    }

    private static string SetTime(string args)
    {
        var split = args.Split(':');
        Game.Instance.TimeManager.Data.Hour = int.Parse(split[0]);
        Game.Instance.TimeManager.Data.Minute = int.Parse(split[1]);
        return $"Time Set To: {Game.Instance.TimeManager.Data.Hour}:{Game.Instance.TimeManager.Data.Minute}";
    }

    private string Burn()
    {
        var structure = Game.Instance.IdService.StructureIdLookup.Values.Where(s => s.Flammable()).GetRandomItem();
        structure.AddBehaviour<Wildfire>();

        return $"Started a fire at {structure.Cell}";
    }

    private void DrawDebugConsole()
    {
        var e = Event.current;
        if (e.keyCode == KeyCode.Return)
        {
            ProcessCommand();
        }
        else if (e.keyCode == KeyCode.Escape)
        {
            Hide();
        }
        else
        {
            var y = 0f;
            y = DrawInputBox(y);

            DrawOutputBox(y);
        }
    }

    private float DrawInputBox(float y)
    {
        var height = 30f;
        GUI.Box(new Rect(0, y, Screen.width, height), "");
        GUI.SetNextControlName("Command");
        _input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20), _input);
        GUI.FocusControl("Command");

        return y + height;
    }

    private void DrawOutputBox(float y)
    {
        if (!string.IsNullOrEmpty(_output))
        {
            var lines = _output.Split('\n');
            var height = 20 * lines.Length;
            var viewPort = new Rect(0, 0, Screen.width - 30, height);

            var rect = new Rect(0, y + 5f, Screen.width, Mathf.Min(100, height));
            GUI.Box(rect, "");
            _scroll = GUI.BeginScrollView(rect, _scroll, viewPort);

            for (var i = 0; i < lines.Length; i++)
            {
                var labelRect = new Rect(5, 20 * i, viewPort.width - 100, 20);
                GUI.Label(labelRect, lines[i]);
            }

            GUI.EndScrollView();
        }
    }

    private string PrintHelp()
    {
        var commandList = "Available commands:\n";
        foreach (var command in Commands)
        {
            commandList += $"\t-{command.Key}\n";
        }
        return commandList;
    }

    private void SetOutput(string output)
    {
        _output = output;
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