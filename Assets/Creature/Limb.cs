using System;
using System.Collections.Generic;

public class Limb
{
    public Dictionary<DamageType, int> Resistance { get; set; }

    public Limb(string name, int hp, params (DamageType damageType, int amount)[] resistances)
    {
        Name = name;
        HP = hp;
        Max = hp;

        Resistance = new Dictionary<DamageType, int>();

        foreach (var (damageType, amount) in resistances)
        {
            Resistance.Add(damageType, amount);
        }

        OffensiveActions = new List<OffensiveActionBase>();
        DefensiveActions = new List<DefensiveActionBase>();
        BoostActions = new List<BuffBase>();
    }

    public bool Busy { get; set; }
    public List<DefensiveActionBase> DefensiveActions { get; set; }
    public List<BuffBase> BoostActions { get; set; }
    public int HP { get; set; }
    public int Max { get; set; }
    public string Name { get; set; }
    public List<OffensiveActionBase> OffensiveActions { get; set; }
    public Creature Owner { get; set; }

    public bool Vital { get; set; }

    public bool Enabled { get; set; } = true;

    public float Percentage
    {
        get
        {
            return Math.Max(0, HP / (float)Max);
        }
    }

    internal void AddDefensiveAction(DefensiveActionBase defensiveAction)
    {
        defensiveAction.Limb = this;
        DefensiveActions.Add(defensiveAction);
    }

    internal void AddBoostAction(BuffBase boostAction)
    {
        boostAction.Limb = this;
        BoostActions.Add(boostAction);
    }

    internal void AddOffensiveAction(OffensiveActionBase offensiveAction)
    {
        offensiveAction.Limb = this;
        OffensiveActions.Add(offensiveAction);
    }

    public int GetDamageAfterResistance(DamageType type, int damage)
    {
        if (Resistance.ContainsKey(type))
        {
            return Math.Max(0, damage - Resistance[type]);
        }

        return damage;
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

                Game.VisualEffectController.SpawnSpriteEffect(null, Owner.Cell, OrderSelectionController.AttackIcon, 1f);
                Game.CreatureController.DestroyCreature(Owner.CreatureRenderer);
            }
            else
            {
                Enabled = false;
            }
        }
    }

    internal void Update(float timeDelta)
    {
    }

    public override string ToString()
    {
        return $"{Owner.Name}: {Name} [{HP}/{Max}]";
    }
}