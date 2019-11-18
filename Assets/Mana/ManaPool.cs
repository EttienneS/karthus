using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ManaPool : Dictionary<ManaColor, Mana>
{
    public string EntityId;
    private IEntity _entity;

    public ManaPool()
    {
        // do not use
    }

    public ManaPool(IEntity owner)
    {
        _entity = owner;
        EntityId = owner.Id;
    }

    [JsonIgnore]
    public IEntity Entity
    {
        get
        {
            if (_entity == null)
            {
                _entity = IdService.GetEntity(EntityId);
            }
            return _entity;
        }
    }
    public void BurnMana(ManaColor color, int amount)
    {
        if (Entity?.Cell != null)
        {
            var effect = Game.VisualEffectController
                .SpawnSpriteEffect(Entity, Entity.Vector, color.ToString(), GameConstants.ChannelDuration);
            effect.Tiny();
            effect.Fades(true);
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

    public void GainMana(ManaColor color, int amount)
    {
        if (Entity?.Cell != null)
        {
            var effect =
               Game.VisualEffectController
                   .SpawnSpriteEffect(Entity, Entity.Vector, color.ToString(), GameConstants.ChannelDuration);
            effect.Tiny();
            effect.Fades();
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

    internal void BurnMana(Dictionary<ManaColor, int> manaCost)
    {
        foreach (var kvp in manaCost)
        {
            BurnMana(kvp.Key, kvp.Value);
        }
    }

    internal bool Empty()
    {
        return Values.All(v => v.Total <= 0);
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
                max = kvp.Value.Total;
            }
        }
        return most;
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
    internal bool HasMana(ManaColor value)
    {
        return ContainsKey(value) && this[value].Total > 0;
    }
    internal bool HasMana(ManaColor color, int amount)
    {
        if (!ContainsKey(color))
        {
            return false;
        }

        return this[color].Total >= amount;
    }

    internal void InitColor(ManaColor color, int start, int max, int desired)
    {
        GainMana(color, start);
        this[color].Attunement = max;
        this[color].Desired = desired;
    }

    internal void Release()
    {
        var cells = new List<Cell> { Entity.Cell };
        cells.AddRange(Entity.Cell.Neighbors.Where(n => n != null));

        var orderedCells = cells.OrderByDescending(c => c.LiquidLevel).ToList(); ;
        foreach (var mana in this.OrderBy(m => m.Value.Total))
        {
            if (mana.Value.Total > 0)
            {
                var matching = orderedCells.Find(c => c.Liquid.HasValue && c.Liquid.Value == mana.Key);
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
}