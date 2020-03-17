using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Limb
{
    public Limb(string name, params (DamageType damageType, int amount)[] resistances)
    {
        Name = name;

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
        Wounds = new List<Wound>();
        DamageThreshold = new DamageThreshold();
    }

    public List<BuffBase> BuffActions { get; set; }
    public bool Busy { get; set; }
    public DamageThreshold DamageThreshold { get; set; }
    public List<DefensiveActionBase> DefensiveActions { get; set; }

    public bool Disabled { get; set; }


    public string Name { get; set; }

    public List<OffensiveActionBase> OffensiveActions { get; set; }

    [JsonIgnore]
    public Creature Owner { get; set; }

    public Dictionary<DamageType, int> Resistance { get; set; }

    public bool Vital { get; set; }

    public List<Wound> Wounds { get; set; }

    public void CheckDeath()
    {
        if (Disabled)
            return;

        var chance = GetChanceOfDeath();

        if (Random.value < chance)
        {
            Owner.Log($"-- {Owner.Name}'s {Name} has been disabled!! --");

            if (Vital)
            {
                Owner.Log($"{Owner.Name} succumbs to its damage!");
                Owner.Dead = true;

                Owner.ManaPool.Release();

                Game.VisualEffectController.SpawnSpriteEffect(null, Owner.Vector, "skull_02_t", 3f);
                Game.VisualEffectController.SpawnLightEffect(null, Owner.Vector, Color.red, 1f, 1f, 3f);
                Game.CreatureController.DestroyCreature(Owner.CreatureRenderer);
            }
        }
    }

    public int GetDamageAfterResistance(DamageType type, int damage)
    {
        if (Resistance.ContainsKey(type))
        {
            return Math.Max(0, damage - Resistance[type]);
        }

        return damage;
    }

    public string GetState()
    {
        if (Disabled)
        {
            return "- Disabled -";
        }

        var chance = GetChanceOfDeath();

        if (chance > 0.1f)
        {
            return "Critical";
        }
        else if (chance > 0.07f)
        {
            return "Severe";
        }
        else if (chance > 0.03f)
        {
            return "Wounded";
        }
        else if (chance > 0.01f)
        {
            return "Hurt";
        }
        else
        {
            return "Fine";
        }
    }

    public override string ToString()
    {
        var msg = $"State: {GetState()}\n";

        foreach (var wound in Wounds.GroupBy(w => w.ToString()).Select(w => new { Text = w.Key, Count = w.Count() }))
        {
            msg += $"\t\t{wound.Count}x {wound.Text}\n";
        }
        return msg.Trim();
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

    internal void Damage(string source, DamageType type, int inputDamage)
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

        Wounds.Add(new Wound(this, source, type, DamageThreshold.GetSeverity(type, damage)));

        Owner.Log($"*{Owner.Name}'s {Name} takes [{damage}] damage*");

        Owner.Aggression += (damage / 20.0f);
    }

    internal void Link(Creature owner)
    {
        Owner = owner;

        OffensiveActions.ForEach(a => a.Limb = this);
        DefensiveActions.ForEach(a => a.Limb = this);
        BuffActions.ForEach(a => a.Limb = this);
        Wounds.ForEach(w => w.Limb = this);
    }

    internal void Update(float timeDelta)
    {
        foreach (var wound in Wounds.ToList())
        {
            if (wound.Healed())
            {
                Owner.Log($"{wound.GetName()} healed completely");
                Wounds.Remove(wound);
            }
            else
            {
                wound.Update(timeDelta);
            }
        }

        CheckDeath();
    }

    public float GetChanceOfDeath()
    {
        var total = 0f;

        foreach (var wound in Wounds)
        {
            total += wound.Danger;
        }

        total -= Owner.Vitality;

        var chance = 0f;
        if (total > 0)
        {
            chance = total * 0.005f;
        }

        return chance;
    }
}