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
                _entity = EntityId.GetEntity();
            }
            return _entity;
        }
    }

    internal bool HasMana(KeyValuePair<ManaColor, float> mana)
    {
        return HasMana(new Dictionary<ManaColor, float> { { mana.Key, mana.Value } });
    }

    public void BurnMana(ManaColor color, float amount)
    {
        if (Entity?.Cell != null)
        {
            var effect = Game.VisualEffectController.SpawnSpriteEffect(Entity, Entity.Vector, color.ToString(), GameConstants.ChannelDuration);
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
    }

    public void GainMana(ManaColor color, float amount)
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

    public Mana GetBaseMana(ManaColor color, float startAmount)
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

    public float ManaCount()
    {
        var count = 0f;
        foreach (var kvp in this)
        {
            count += kvp.Value.Total;
        }

        return count;
    }

    public override string ToString()
    {
        return $"R:{GetTotal(ManaColor.Red)}, G:{GetTotal(ManaColor.Green)}, U:{GetTotal(ManaColor.Blue)}, B:{GetTotal(ManaColor.Black)}, W:{GetTotal(ManaColor.White)}";
    }

    internal void BurnMana(Dictionary<ManaColor, float> manaCost)
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

    internal float GetTotal(ManaColor color)
    {
        if (!ContainsKey(color))
        {
            return 0;
        }
        return this[color].Total;
    }

    internal void InitColor(ManaColor color, int start, int max, int desired)
    {
        GainMana(color, start);
        this[color].Attunement = max;
        this[color].Desired = desired;
    }

    internal void Release()
    {
        foreach (var mana in this.OrderBy(m => m.Value.Total))
        {
            var amount = Mathf.FloorToInt(mana.Value.Total);

            if (amount > 0)
            {
                Game.VisualEffectController.SpawnLightEffect(null, Entity.Vector, mana.Key.GetActualColor(), amount, amount * 2, 5);
            }
        }
    }

    internal bool HasMana(Dictionary<ManaColor, float> manaCost)
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

    internal bool HasMana(ManaColor color, float amount)
    {
        if (!ContainsKey(color))
        {
            return false;
        }

        return this[color].Total >= amount;
    }

    public string GetString()
    {
        var str = "";
        foreach (var kvp in this)
        {
            switch (kvp.Key)
            {
                case ManaColor.Black:
                    str += "B:";
                    break;

                case ManaColor.Blue:
                    str += "U:";
                    break;

                case ManaColor.Red:
                    str += "R:";
                    break;

                case ManaColor.White:
                    str += "W:";
                    break;

                case ManaColor.Green:
                    str += "G:";
                    break;
            }
            str += kvp.Value.Total + ", ";
        }

        return str.Trim().Trim(',');
    }

    internal ManaColor GetManaWithMost()
    {
        var most = ManaColor.Blue;
        var max = float.MinValue;

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
}