using System;
using UnityEngine;

public class Mana
{
    public Mana(ManaColor color, Action<float> castAction, Action<float> gainAction, Action<float> burnAction)
    {
        CastAction = castAction;
        GainAction = gainAction;
        BurnAction = burnAction;
        Color = color;
    }

    public Action<float> BurnAction { get; set; }
    public Action<float> CastAction { get; set; }
    public ManaColor Color { get; set; }
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

    internal Color GetActualColor()
    {
        switch (Color)
        {
            case ManaColor.Green:
                return new Color(0, 0.6f, 0);

            case ManaColor.Blue:
                return UnityEngine.Color.blue;

            case ManaColor.Black:
                return UnityEngine.Color.black;

            case ManaColor.White:
                return UnityEngine.Color.white;

            case ManaColor.Red:
                return UnityEngine.Color.red;

            default:
                throw new NotImplementedException();
        }
    }
}