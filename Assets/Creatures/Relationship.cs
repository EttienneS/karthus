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

    public Relationship(IEntity entity) : this()
    {
        Entity = entity;
    }

    [JsonIgnore]
    public IEntity Entity
    {
        get
        {
            return EntityId.GetEntity();
        }
        set
        {
            EntityId = value.Id;
        }
    }

    public string EntityId { get; set; }

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