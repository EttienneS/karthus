using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Mobility
{
    Walk, Fly
}

public class CreatureData : IEntity, IMagicAttuned
{
    public const string SelfKey = "Self";
    public string BehaviourName;

    public Coordinates Coordinates { get; set; }

    public string Id { get; set; }

    [JsonIgnore]
    public Behaviours.GetBehaviourTaskDelegate GetBehaviourTask;

    public Dictionary<string, Memory> Mind = new Dictionary<string, Memory>();
    public Mobility Mobility;
    public Direction MoveDirection = Direction.S;
    public string Name;

    public float Speed = 10f;
    public Dictionary<string, string> StringProperties = new Dictionary<string, string>();
    public Dictionary<string, float> ValueProperties = new Dictionary<string, float>();

    internal float InternalTick;

    [JsonIgnore]
    internal Coordinates LastPercievedCoordinate;

    internal float WorkTick;

    [JsonIgnore]
    private List<CellData> _awareness;

    public List<CellData> Awareness
    {
        get
        {
            if (_awareness == null && Coordinates != null)
            {
                _awareness = Game.MapGrid.GetCircle(Coordinates, Perception);
            }

            return _awareness;
        }
    }

    [JsonIgnore]
    public CellData CurrentCell
    {
        get
        {
            return Game.MapGrid.GetCellAtCoordinate(Coordinates);
        }
    }

    [JsonIgnore]
    public Faction Faction
    {
        get
        {
            return FactionController.Factions[FactionName];
        }
    }

    public string FactionName { get; set; }

    [JsonIgnore]
    public CreatureRenderer CreatureRenderer
    {
        get
        {
            return Game.CreatureController.GetCreatureForCreatureData(this);
        }
    }

    public ManaPool ManaPool { get; set; } = new ManaPool();

    public int Perception { get; set; }

    [JsonIgnore]
    public Memory Self
    {
        get
        {
            if (!Mind.ContainsKey(SelfKey))
            {
                Mind.Add(SelfKey, new Memory());
            }

            return Mind[SelfKey];
        }
    }

    public string Sprite { get; set; }

    [JsonIgnore]
    public TaskBase Task { get; set; }

    public static CreatureData Load(string creatureData)
    {
        return JsonConvert.DeserializeObject<CreatureData>(creatureData, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });
    }

    internal void Forget(string context)
    {
        // Debug.Log($"Forget context: {context}");
        // if !LongTerm?
        Mind.Remove(context);
    }

    internal void FreeResources(string context)
    {
        if (!Mind.ContainsKey(context))
        {
            // already forgot about this context, do nothing
            return;
        }

        // see if character remembers any structures used in this current task context
        // if any exist and they were reserved by this creature, free them
        if (Mind[context].ContainsKey(MemoryType.Structure))
        {
            foreach (var structureId in Mind[context][MemoryType.Structure])
            {
                var structure = IdService.GetStructureFromId(structureId);
                if (structure.InUseBy == Id)
                {
                    structure.Free();
                }
            }
        }
    }

    internal void Know(string context)
    {
        // Debug.Log($"Add context: {context}");
        Mind.Add(context, new Memory());
    }

    internal void Live()
    {
        ValueProperties[Prop.Hunger] += Random.value;
        ValueProperties[Prop.Energy] -= Random.Range(0.1f, 0.25f);
    }

    internal void Perceive()
    {
        if (LastPercievedCoordinate != Coordinates)
        {
            _awareness = null;
            LastPercievedCoordinate = Coordinates;
        }
    }

    internal bool Update(float timeDelta)
    {
        if (Game.TimeManager.Paused)
            return false;

        InternalTick += timeDelta;
        WorkTick += timeDelta;

        if (WorkTick >= Game.TimeManager.WorkInterval)
        {
            WorkTick = 0;
            ProcessTask();
        }

        if (InternalTick >= Game.TimeManager.TickInterval)
        {
            InternalTick = 0;
            Perceive();
            Live();

            return true;
        }

        return false;
    }

    internal void UpdateMemory(string context, MemoryType memoryType, string info)
    {
        // Debug.Log($"Remember: {context}, {memoryType}: '{info}'");
        Mind[context].AddInfo(memoryType, info);
    }

    internal void UpdateSelfMemory(MemoryType memoryType, string info)
    {
        UpdateMemory(SelfKey, memoryType, info);
    }

    private void ProcessTask()
    {
        if (Task == null)
        {
            var task = Faction.GetTask(this);
            var context = $"{Id} - {task} - {Game.TimeManager.Now}";

            Know(context);
            task.Context = context;

            Faction.AssignTask(this, task);
            Task = task;
        }
        else
        {
            try
            {
                Faction.AssignTask(this, Task);

                if (Task.Done())
                {
                    Task.ShowDoneEmote();
                    FreeResources(Task.Context);
                    Forget(Task.Context);

                    Faction.TaskComplete(Task);
                    Task = null;
                }
            }
            catch (TaskFailedException ex)
            {
                Debug.LogWarning($"Task failed: {ex}");
                Faction.TaskFailed(Task, ex.Message);
            }
        }
    }
}