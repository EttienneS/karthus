using System;
using UnityEngine;

public class Mana
{
    public Mana(ManaColor color, Action<float> gainAction, Action<float> burnAction)
    {
        GainAction = gainAction;
        BurnAction = burnAction;
        Color = color;
    }

    public Action<float> BurnAction { get; set; }
    public ManaColor Color { get; set; }
    public Action<float> GainAction { get; set; }

    public float Total { get; set; }

    public void Burn(float amount)
    {
        Total -= amount;
        BurnAction?.Invoke(amount);
    }

    public void Gain(float amount)
    {
        Total += amount;
        GainAction?.Invoke(amount);
    }

}