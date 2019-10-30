using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Mobility
{
    Walk, AbyssWalk, Fly
}

public class Limb
{
    public int HP { get; set; }

    public int Max { get; set; }

    public string Name { get; set; }
}

public class CreatureData : IEntity
{
    public List<VisualEffectData> LinkedVisualEffects { get; set; } = new List<VisualEffectData>();

    // rather than serialzing the cell object we keep this lazy link for load
    public (int X, int Y) Coords = (-1, -1);

    public const string SelfKey = "Self";

    [JsonIgnore] public Color BottomColor;

    public Direction Facing = Direction.S;

    [JsonIgnore] public Behaviours.GetBehaviourTaskDelegate GetBehaviourTask;
    [JsonIgnore] public Color HairColor;

    public int HairStyle;
    public Dictionary<string, Memory> Mind = new Dictionary<string, Memory>();
    public Mobility Mobility;

    [JsonIgnore] public Color SkinColor;

    public float Speed = 10f;

    [JsonIgnore] public Color TopColor;

    internal float InternalTick;

    [JsonIgnore] internal Cell LastPercievedCoordinate;

    internal float WorkTick;

    private List<Cell> _awareness;

    private Cell _cell;

    private Faction _faction;
    private bool _firstRun = true;

    private CreatureTask _task;

    public List<Limb> Body { get; set; }

    [JsonIgnore]
    public List<Cell> Awareness
    {
        get
        {
            if (_awareness == null && Cell != null)
            {
                _awareness = Game.Map.GetCircle(Cell, Perception);
            }

            return _awareness;
        }
    }

    public string BehaviourName { get; set; }

    [JsonIgnore]
    public Cell Cell
    {
        get
        {
            if (_cell == null && Coords.X >= 0 && Coords.Y >= 0)
            {
                _cell = Game.Map.GetCellAtCoordinate(Coords.X, Coords.Y);
                _cell.AddCreature(this);
            }
            return _cell;
        }
        set
        {
            if (_cell != null)
            {
                _cell.RemoveCreature(this);
            }

            if (value != null)
            {
                _cell = value;
                _cell.AddCreature(this);

                Coords = (_cell.X, _cell.Y);
            }
        }
    }

    [JsonIgnore]
    public CreatureRenderer CreatureRenderer { get; set; }

    internal IEntity GetClosestBattery()
    {
        return Faction.GetClosestBattery(this);
    }

    [JsonIgnore]
    public Faction Faction
    {
        get
        {
            if (_faction == null)
            {
                _faction = Game.FactionController.Factions[FactionName];
            }

            return _faction;
        }
    }

    public string FactionName { get; set; }
    public string Id { get; set; }
    public ManaPool ManaPool { get; set; }
    public string Name { get; set; }
    public int Perception { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

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

    public List<Skill> Skills { get; set; }
    public string Sprite { get; set; }

    public CreatureTask Task
    {
        get
        {
            return _task;
        }
        set
        {
            if (_task != null)
            {
                _task.Destroy();
            }
            _task = value;
        }
    }

    public Dictionary<string, float> ValueProperties { get; set; } = new Dictionary<string, float>();


    public void Damage(int amount, ManaColor type)
    {
        if (!ManaPool.Empty())
        {
            for (var i = amount; i > 0; i--)
            {
                var mana = ManaPool.GetRandomManaColorFromPool();
                ManaPool.BurnMana(mana, i);
            }
        }
        else
        {
            //Game.EffectController.SpawnEffect(Cell, 1);
        }
    }

    public void Face(Cell cell)
    {
        if (cell.Y < Cell.Y)
        {
            Facing = Direction.S;
        }
        else if (cell.Y > Cell.Y)
        {
            Facing = Direction.N;
        }
        else if (cell.X < Cell.X)
        {
            Facing = Direction.W;
        }
        else if (cell.X > Cell.X)
        {
            Facing = Direction.E;
        }
        else
        {
            // default
            Facing = Direction.S;
        }
    }

    public void GainSkill(string skillName, float amount)
    {
        var skill = GetSkill(skillName);

        if (skill != null)
        {
            skill = new Skill(skillName);
            Skills.Add(skill);
        }

        skill.Level += amount;
    }

    public Skill GetSkill(string skillName)
    {
        return Skills.Find(s => s.Name == skillName);
    }

    public float GetSkillLevel(string skillName)
    {
        var skill = GetSkill(skillName);

        if (skill == null)
        {
            return 5; // untyped
        }

        if (skill.Enabled != true)
        {
            return float.MinValue;
        }

        return skill.Level;
    }

    public bool HasSkill(string skillName)
    {
        if (string.IsNullOrEmpty(skillName))
        {
            return true; // unskilled
        }
        var skill = GetSkill(skillName);
        return skill?.Enabled == true;
    }

    public void SetColors()
    {
        if (_firstRun)
        {
            SkinColor = ColorExtensions.GetRandomSkinColor();
            HairColor = ColorExtensions.GetRandomHairColor();
            TopColor = ColorExtensions.GetRandomColor();
            BottomColor = ColorExtensions.GetRandomColor();
            HairStyle = Random.Range(1, 3);
            _firstRun = false;
        }

        CreatureRenderer.BodyRenderer.color = SkinColor;
        CreatureRenderer.TopRenderer.color = TopColor;
        CreatureRenderer.BottomRenderer.color = BottomColor;
        CreatureRenderer.HairRenderer.color = HairColor;
    }

    internal void CancelTask()
    {
        Faction.RemoveTask(Task);
        Task.Destroy();
        Task = null;
    }

    internal bool CanDo(CreatureTask t)
    {
        if (HasSkill(t.RequiredSkill))
        {
            return GetSkillLevel(t.RequiredSkill) >= t.RequiredSkillLevel;
        }

        return false;
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
                var structure = IdService.GetStructure(structureId);
                if (structure != null && structure.InUseBy == this)
                {
                    structure.Free();
                }
            }
        }
    }

    internal int GetPriority(CreatureTask t)
    {
        if (string.IsNullOrEmpty(t.RequiredSkill))
        {
            return 5; // untyped return baseline
        }

        if (HasSkill(t.RequiredSkill))
        {
            return GetSkill(t.RequiredSkill).Priority;
        }

        return 0;
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
        if (LastPercievedCoordinate != Cell)
        {
            _awareness = null;
            LastPercievedCoordinate = Cell;
        }
    }

    internal bool Update(float timeDelta)
    {
        if (Game.TimeManager.Paused)
            return false;

        InternalTick += timeDelta;
        WorkTick += timeDelta;

        if (ManaPool.Empty())
        {
            Game.VisualEffectController.SpawnEffect(this, Cell, 0.1f);
            Game.VisualEffectController.SpawnEffect(this, Cell, 0.1f);
            Game.VisualEffectController.SpawnEffect(this, Cell, 0.1f);

            Game.CreatureController.DestroyCreature(CreatureRenderer);
            return false;
        }

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
            UpdateSprite();

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
            var task = Faction.TakeTask(this);

            if (task != null)
            {
                var context = $"{Id} - {task} - {Game.TimeManager.Now}";

                Know(context);
                task.Context = context;

                Task = task;
            }
        }
        else
        {
            try
            {
                if (Task.Done(this))
                {
                    Task.ShowDoneEmote(this);
                    FreeResources(Task.Context);
                    Forget(Task.Context);

                    Faction.RemoveTask(Task);
                    Task = null;
                }
                else
                {
                    if (Random.value > 0.8)
                    {
                        Task.ShowBusyEmote(this);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Task failed: {ex}");
                CancelTask();
            }
        }
    }

    public int Index = 0;

    private void UpdateSprite()
    {
        bool flip = Facing == Direction.W || Facing == Direction.NW || Facing == Direction.SW;
        if (!Sprite.Contains("_"))
        {
            CreatureRenderer.MainRenderer.flipX = flip;
            CreatureRenderer.MainRenderer.sprite = Game.SpriteStore.GetCreatureSprite(Sprite, ref Index);

        }
        else
        {
            var facingKey = Game.SpriteStore.FacingUp(Facing) ? "b_" : "f_";

            CreatureRenderer.FaceRenderer.flipX = flip;
            CreatureRenderer.BodyRenderer.flipX = flip;
            CreatureRenderer.TopRenderer.flipX = flip;
            CreatureRenderer.BottomRenderer.flipX = flip;
            CreatureRenderer.HairRenderer.flipX = flip;

            SetColors();

            if (facingKey == "f_")
            {
                CreatureRenderer.FaceRenderer.sprite = Game.SpriteStore.GetBodySprite(Sprite + "face");
            }
            else
            {
                CreatureRenderer.FaceRenderer.sprite = null;
            }
            CreatureRenderer.BodyRenderer.sprite = Game.SpriteStore.GetBodySprite(Sprite + facingKey + "body");
            CreatureRenderer.TopRenderer.sprite = Game.SpriteStore.GetBodySprite(Sprite + facingKey + "top");
            CreatureRenderer.BottomRenderer.sprite = Game.SpriteStore.GetBodySprite(Sprite + facingKey + "bottom");
            CreatureRenderer.HairRenderer.sprite = Game.SpriteStore.GetBodySprite(Sprite + facingKey + "hair_" + HairStyle);
        }
    }

    public void Damage(IEntity attacker, TargetType target, float power, float accuracy)
    {
        throw new NotImplementedException();
    }
}

public class Skill
{
    private int _priority;

    public Skill(string name)
    {
        Name = name;
        Level = 0f;
        Enabled = true;
        Priority = 5;
    }

    public bool Enabled { get; set; }
    public float Level { get; set; }
    public string Name { get; set; }

    public int Priority
    {
        get
        {
            return _priority;
        }
        set
        {
            _priority = Mathf.Clamp(value, 1, 10);
        }
    }

    public override string ToString()
    {
        var skill = $"{Name}: {Level.ToString("N2")} [{Priority}]";

        if (!Enabled)
        {
            skill = $"-{skill}-";
        }

        return skill;
    }
}