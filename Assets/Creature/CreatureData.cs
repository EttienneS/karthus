﻿using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class CreatureData
{
    public const string SelfKey = "Self";
    public Coordinates Coordinates;

    [JsonIgnore]
    public Behaviours.GetBehaviourTaskDelegate GetBehaviourTask;

    public int Id;
    public Dictionary<string, Memory> Mind = new Dictionary<string, Memory>();
    public Direction MoveDirection = Direction.S;
    public string Name;

    public float Speed = 10f;
    public Dictionary<string, string> StringProperties = new Dictionary<string, string>();
    public Dictionary<string, float> ValueProperties = new Dictionary<string, float>();

    public Dictionary<ManaColor, Mana> ManaPool = new Dictionary<ManaColor, Mana>();

    public void BurnMana(ManaColor manaColor)
    {
        LinkedGameObject.SetTempMaterial(Game.MaterialController.GetChannelingMaterial(manaColor.GetActualColor()), 0.5f);
        ManaPool.BurnMana(manaColor, 1);
    }

    public void GainMana(ManaColor manaColor)
    {
        LinkedGameObject.SetTempMaterial(Game.MaterialController.GetChannelingMaterial(manaColor.GetActualColor()), 0.5f);
        ManaPool.GainMana(manaColor, 1);
    }

    internal float InternalTick;

    [JsonIgnore]
    public CellData CurrentCell
    {
        get
        {
            return Game.MapGrid.GetCellAtCoordinate(Coordinates);
        }
    }

    public string FactionName { get; set; }

    [JsonIgnore]
    public Faction Faction
    {
        get
        {
            return FactionController.Factions[FactionName];
        }
    }

    [JsonIgnore]
    public Creature LinkedGameObject
    {
        get
        {
            return Game.CreatureController.GetCreatureForCreatureData(this);
        }
    }

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
        var data = JsonConvert.DeserializeObject<CreatureData>(creatureData, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });
        return data;
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
                if (structure.InUseBy == this.GetGameId())
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

    internal void UpdateMemory(string context, MemoryType memoryType, string info)
    {
        // Debug.Log($"Remember: {context}, {memoryType}: '{info}'");
        Mind[context].AddInfo(memoryType, info);
    }

    internal void UpdateSelfMemory(MemoryType memoryType, string info)
    {
        UpdateMemory(SelfKey, memoryType, info);
    }
}