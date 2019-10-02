using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ManaPool : Dictionary<ManaColor, Mana>
{
    public IEntity Entity;

    public ManaPool(IEntity owner)
    {
        Entity = owner;
    }

    internal bool HasMana(Dictionary<ManaColor, int> manaCost)
    {
        foreach (var kvp in manaCost)
        {
            if (!HasMana(kvp.Key, kvp.Value))
            {
                return false;
            }
        }

        return true;
    }

    internal void BurnMana(Dictionary<ManaColor, int> manaCost)
    {
        foreach (var kvp in manaCost)
        {
            BurnMana(kvp.Key, kvp.Value);
        }
    }

    internal ManaColor GetManaWithMost()
    {
        var most = ManaColor.Blue;
        var max = int.MinValue;
        foreach (var kvp in this)
        {
            if (kvp.Value.Total > max)
            {
                most = kvp.Key;
            }
        }
        return most;
    }

    internal bool HasMana(ManaColor value)
    {
        return ContainsKey(value) && this[value].Total > 0;
    }

    public void BurnMana(ManaColor color, int amount)
    {
        if (Entity?.Cell != null)
        {
            Game.EffectController
                .SpawnSpriteEffect(Entity.Cell, color.ToString(), 0.5f)
                .Tiny()
                .FadeDown();

        }

        if (!ContainsKey(color))
        {
            Debug.LogWarning("Unable to burn mana you do not have!");
        }
        else
        {
            if (this[color].Total < amount)
            {
                Debug.LogWarning("Not enough mana!");
                return;
            }
            this[color].Burn(amount);
        }

        //if (Empty())
        //{
        //    Debug.Log("No mana, destroy!");
        //    IdService.DestroyEntity(Entity);
        //}
    }

    internal void Transfer(ManaPool target, ManaColor manaColor, int amount)
    {
        if (amount > 0)
        {
            BurnMana(manaColor, amount);
            target.GainMana(manaColor, amount);
        }
        else
        {
            target.BurnMana(manaColor, amount);
            GainMana(manaColor, amount);
        }
    }

    internal bool HasMana(ManaColor color, int amount)
    {
        if (!ContainsKey(color))
        {
            return false;
        }

        return this[color].Total >= amount;
    }

    public void GainMana(ManaColor color, int amount)
    {
        if (Entity?.Cell != null)
        {
            Game.EffectController
                .SpawnSpriteEffect(Entity.Cell, color.ToString(), 0.5f)
                .Tiny()
                .FadeUp();
        }

        if (!ContainsKey(color))
        {
            Add(color, GetBaseMana(color, amount));
        }
        else
        {
            this[color].Total += amount;
        }
    }

    public Mana GetBaseMana(ManaColor color, int startAmount)
    {
        switch (color)
        {
            case ManaColor.Red:
                return RedMana.GetBase(startAmount);

            case ManaColor.Green:
                return GreenMana.GetBase(startAmount);

            case ManaColor.Blue:
                return BlueMana.GetBase(startAmount);

            case ManaColor.Black:
                return BlackMana.GetBase(startAmount);

            case ManaColor.White:
                return WhiteMana.GetBase(startAmount);

            default:
                throw new KeyNotFoundException();
        }
    }

    public ManaColor GetRandomManaColorFromPool()
    {
        var tmp = this.Where(k => k.Value.Total > 0).Select(k => k.Key).Distinct().ToList();
        return tmp[Mathf.FloorToInt(Random.value * tmp.Count)];
    }

    public int ManaCount()
    {
        var count = 0;
        foreach (var kvp in this)
        {
            count += kvp.Value.Total;
        }

        return count;
    }

    internal bool Empty()
    {
        return Values.All(v => v.Total <= 0);
    }

    internal void InitColor(ManaColor color, int start, int max, int desired)
    {
        GainMana(color, start);
        this[color].Max = max;
        this[color].Desired = desired;
    }
}