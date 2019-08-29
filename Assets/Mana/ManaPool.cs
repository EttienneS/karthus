using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ManaPool : Dictionary<ManaColor, Mana>
{
    public void BurnMana(ManaColor color, int amount)
    {
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