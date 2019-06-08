using System;

public class GreenMana
{
    public static Mana GetBase(float startingTotal = 0)
    {
        return new Mana(ManaColor.Green, CastGreen, GainGreen, BurnGreen)
        {
            Total = startingTotal,
        };
    }

    public static void CastGreen(float amount)
    {
    }

    public static void BurnGreen(float amount)
    {
    }

    public static void GainGreen(float amount)
    {
    }
}

public enum ManaColor
{
    Red, Green, Blue, White, Black
}

public class Mana
{
    public Mana(ManaColor color, Action<float> castAction, Action<float> gainAction, Action<float> burnAction)
    {
        CastAction = castAction;
        GainAction = gainAction;
        BurnAction = burnAction;
        Color = color;
    }

    public ManaColor Color { get; set; }

    public Action<float> BurnAction { get; set; }
    public Action<float> CastAction { get; set; }
    public Action<float> GainAction { get; set; }

    public float Total { get; set; }

    public void Burn(float amount)
    {
        Total -= amount;
        BurnAction?.Invoke(amount);
    }

    public void Cast(float amount)
    {
        Total -= amount;
        CastAction?.Invoke(amount);
    }

    public void Gain(float amount)
    {
        Total += amount;
        GainAction?.Invoke(amount);
    }
}