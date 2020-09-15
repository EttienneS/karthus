using Assets.Creature;
using Assets.Creature.Behaviour;
using Needs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Relationship
{
    public List<(string name, float value)> Effectors = new List<(string name, float value)>();

    public Relationship()
    {
    }

    public Relationship(CreatureData creature) : this()
    {
        Creature = creature;
    }

    [JsonIgnore]
    public CreatureData Creature
    {
        get
        {
            return CreatureId.GetCreature();
        }
        set
        {
            CreatureId = value.Id;
        }
    }

    public string CreatureId { get; set; }

    [JsonIgnore]
    public float Value
    {
        get
        {
            var total = 0f;

            foreach (var (name, value) in Effectors)
            {
                total += value;
            }

            return total;
        }
    }

    internal void AddEffect(string name, float value)
    {
        Effectors.Add((name, value));
    }
}