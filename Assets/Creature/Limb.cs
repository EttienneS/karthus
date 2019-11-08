using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Limb
{
    public Limb(string name, int hp, params (DamageType damageType, int amount)[] resistances)
    {
        Name = name;
        HP = hp;
        Max = hp;

        Resistance = new Dictionary<DamageType, int>();

        if (resistances != null)
        {
            foreach (var (damageType, amount) in resistances)
            {
                Resistance.Add(damageType, amount);
            }
        }

        OffensiveActions = new List<OffensiveActionBase>();
        DefensiveActions = new List<DefensiveActionBase>();
        BuffActions = new List<BuffBase>();
    }

    public List<BuffBase> BuffActions { get; set; }

    public bool Busy { get; set; }
    public List<DefensiveActionBase> DefensiveActions { get; set; }

    [JsonIgnore]
    public bool Enabled
    {
        get
        {
            return HP > 0;
        }
    }

    public int HP { get; set; }
    public int Max { get; set; }
    public string Name { get; set; }
    public List<OffensiveActionBase> OffensiveActions { get; set; }

    [JsonIgnore]
    public Creature Owner { get; set; }

    [JsonIgnore]
    public float Percentage
    {
        get
        {
            return Math.Max(0, HP / (float)Max);
        }
    }

    public Dictionary<DamageType, int> Resistance { get; set; }
    public bool Vital { get; set; }

    public int GetDamageAfterResistance(DamageType type, int damage)
    {
        if (Resistance.ContainsKey(type))
        {
            return Math.Max(0, damage - Resistance[type]);
        }

        return damage;
    }

    public override string ToString()
    {
        return $"{Name} [{HP}/{Max}]";
    }

    internal void AddBoostAction(BuffBase boostAction)
    {
        boostAction.Limb = this;
        BuffActions.Add(boostAction);
    }

    internal void AddDefensiveAction(DefensiveActionBase defensiveAction)
    {
        defensiveAction.Limb = this;
        DefensiveActions.Add(defensiveAction);
    }

    internal void AddOffensiveAction(OffensiveActionBase offensiveAction)
    {
        offensiveAction.Limb = this;
        OffensiveActions.Add(offensiveAction);
    }

    internal void Damage(DamageType type, int inputDamage)
    {
        var damage = GetDamageAfterResistance(type, inputDamage);

        if (damage == 0)
        {
            Owner.Log($"-- {Owner.Name}'s {Name} resists all [{inputDamage}] of the incoming damage --");
            return;
        }
        else if (damage < inputDamage)
        {
            Owner.Log($"-- {Owner.Name}'s {Name} resists some [{inputDamage - damage}] of the {inputDamage} incoming damage --");
        }

        HP -= damage;
        Owner.Log($"*{Owner.Name}'s {Name} takes [{damage}] damage ({Math.Floor(Percentage * 100)}%)*");

        Owner.Aggression += (damage / 20.0f);
        if (HP <= 0)
        {
            Owner.Log($"-- {Owner.Name}'s {Name} has been disabled!! --");

            if (Vital)
            {
                Owner.Log($"{Owner.Name} collapses!");
                Owner.Dead = true;

                Game.VisualEffectController.SpawnSpriteEffect(null, Owner.Vector, OrderSelectionController.AttackIcon, 1f);
                Game.VisualEffectController.SpawnLightEffect(null, Owner.Vector, Color.red, 1f, 1f, 1f);
                Game.CreatureController.DestroyCreature(Owner.CreatureRenderer);
            }
        }
    }

    internal void Link(Creature owner)
    {
        Owner = owner;

        OffensiveActions.ForEach(a => a.Limb = this);
        DefensiveActions.ForEach(a => a.Limb = this);
        BuffActions.ForEach(a =>
        {
            a.Limb = this;
        });
    }

    internal void Update(float timeDelta)
    {
    }
}