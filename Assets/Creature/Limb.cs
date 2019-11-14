using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        Wounds = new List<Wound>();
        DamageThreshold = new DamageThreshold();
    }

    public List<BuffBase> BuffActions { get; set; }
    public bool Busy { get; set; }
    public DamageThreshold DamageThreshold { get; set; }
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

    public List<Wound> Wounds { get; set; }

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
        var msg = $"{Name} [{HP}/{Max}]\n";

        foreach (var wound in Wounds.GroupBy(w => w.ToString()).Select(w => new { Text = w.Key, Count = w.Count() }))
        {
            msg += $"\t\t{wound.Count}x {wound.Text}\n";
        }
        return msg;
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

                var cells = new List<Cell>
                {
                    Owner.Cell
                };
                cells.AddRange(Owner.Cell.Neighbors.Where(n => n != null));

                var orderedCells = cells.OrderByDescending(c => c.LiquidLevel).ToList(); ;

                foreach (var mana in Owner.ManaPool.OrderBy(m => m.Value.Total))
                {
                    if (mana.Value.Total > 0)
                    {
                        var matching = orderedCells.FirstOrDefault(c => c.Liquid.HasValue && c.Liquid.Value == mana.Key);
                        if (matching != null)
                        {
                            matching.LiquidLevel += mana.Value.Total;
                        }
                        else
                        {
                            var cell = orderedCells[0];
                            orderedCells.Remove(cell);

                            var amount = mana.Value.Total - cell.LiquidLevel;

                            if (amount > 0)
                            {
                                cell.AddLiquid(mana.Key, amount);
                            }
                            else
                            {
                                cell.LiquidLevel -= amount;
                            }
                        }
                    }
                }

                Game.VisualEffectController.SpawnSpriteEffect(null, Owner.Vector, "skull_02_t", 3f);
                Game.VisualEffectController.SpawnLightEffect(null, Owner.Vector, Color.red, 1f, 1f, 3f);
                Game.CreatureController.DestroyCreature(Owner.CreatureRenderer);
            }
        }
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
        foreach (var wound in Wounds)
        {
            wound.Update(timeDelta);
        }
    }
}
