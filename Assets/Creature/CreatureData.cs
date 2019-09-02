﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Mobility
{
    Walk, AbyssWalk, Fly
}

public class CreatureData : IEntity
{
    public const string SelfKey = "Self";
    public string BehaviourName;

    [JsonIgnore]
    public Color BottomColor;

    public Direction Facing = Direction.S;

    [JsonIgnore]
    public Behaviours.GetBehaviourTaskDelegate GetBehaviourTask;

    [JsonIgnore]
    public Color HairColor;

    public int HairStyle;
    public Dictionary<string, Memory> Mind = new Dictionary<string, Memory>();
    public Mobility Mobility;
    public string Name;
    [JsonIgnore]
    public Color SkinColor;

    public float Speed = 10f;
    public Dictionary<string, string> StringProperties = new Dictionary<string, string>();
    [JsonIgnore]
    public Color TopColor;

    public Dictionary<string, float> ValueProperties = new Dictionary<string, float>();
    internal float InternalTick;

    [JsonIgnore]
    internal Coordinates LastPercievedCoordinate;

    internal float WorkTick;

    [JsonIgnore]
    private List<CellData> _awareness;

    [JsonIgnore]
    private bool _firstRun = true;

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

    public Coordinates Coordinates { get; set; }

    [JsonIgnore]
    public CreatureRenderer CreatureRenderer
    {
        get
        {
            return Game.CreatureController.GetCreatureForCreatureData(this);
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

    public string FactionName { get; set; }
    public string Id { get; set; }
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

    public void Damage(int amount, ManaColor type)
    {
        if (!ManaPool.Empty())
        {
            for (var i = 0; i < amount; i--)
            {
                var mana = ManaPool.GetRandomManaColorFromPool();
                ManaPool.BurnMana(mana, i);
            }
        }
        else
        {
            Game.EffectController.SpawnEffect(Coordinates, 1);
        }
    }

    public void Face(Coordinates coordinates)
    {
        if (coordinates.Y < Coordinates.Y)
        {
            Facing = Direction.S;
        }
        else if (coordinates.Y > Coordinates.Y)
        {
            Facing = Direction.N;
        }
        else if (coordinates.X < Coordinates.X)
        {
            Facing = Direction.W;
        }
        else if (coordinates.X > Coordinates.X)
        {
            Facing = Direction.E;
        }
        else
        {
            // default
            Facing = Direction.S;
        }
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
                if (structure != null && structure.InUseBy == Id)
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

        if (ManaPool.Empty())
        {
            Game.EffectController.SpawnEffect(Coordinates, 3f);
            Game.EffectController.SpawnEffect(Coordinates, 3f);
            Game.EffectController.SpawnEffect(Coordinates, 3f);

            Game.CreatureController.DestroyCreature(CreatureRenderer);
            return false;
        }

        if (WorkTick >= Game.TimeManager.WorkInterval)
        {
            WorkTick = 0;
            ProcessTask();
            UpdateSprite();
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
        var faction = this.GetFaction();
        if (Task == null)
        {
            var task = faction.GetTask(this);
            var context = $"{Id} - {task} - {Game.TimeManager.Now}";

            Know(context);
            task.Context = context;

            faction.AssignTask(this, task);
            Task = task;
        }
        else
        {
            try
            {
                faction.AssignTask(this, Task);

                if (Task.Done())
                {
                    Task.ShowDoneEmote();
                    FreeResources(Task.Context);
                    Forget(Task.Context);

                    faction.TaskComplete(Task);
                    Task = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Task failed: {ex}");
                faction.TaskFailed(Task, ex.Message);
            }
        }
    }
    private void UpdateSprite()
    {
        bool flip = Facing == Direction.W || Facing == Direction.NW || Facing == Direction.SW;
        if (!Sprite.Contains("_"))
        {
            CreatureRenderer.MainRenderer.flipX = flip;
            CreatureRenderer.MainRenderer.sprite = Game.SpriteStore.GetCreatureSprite(Sprite);
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
                CreatureRenderer.FaceRenderer.sprite = Game.SpriteStore.GetCreatureSprite(Sprite + "face");
            }
            else
            {
                CreatureRenderer.FaceRenderer.sprite = null;
            }
            CreatureRenderer.BodyRenderer.sprite = Game.SpriteStore.GetCreatureSprite(Sprite + facingKey + "body");
            CreatureRenderer.TopRenderer.sprite = Game.SpriteStore.GetCreatureSprite(Sprite + facingKey + "top");
            CreatureRenderer.BottomRenderer.sprite = Game.SpriteStore.GetCreatureSprite(Sprite + facingKey + "bottom");
            CreatureRenderer.HairRenderer.sprite = Game.SpriteStore.GetCreatureSprite(Sprite + facingKey + "hair_" + HairStyle);

        }
    }
}