using Newtonsoft.Json;
using System;

public class Mana
{
    public Mana(ManaColor color, Action<int> gainAction, Action<int> burnAction)
    {
        GainAction = gainAction;
        BurnAction = burnAction;
        Color = color;
    }

    [JsonIgnore]
    public Action<int> BurnAction { get; set; }

    public ManaColor Color { get; set; }
    public int Desired { get; set; }

    [JsonIgnore]
    public Action<int> GainAction { get; set; }

    public int Max { get; set; }
    public int Total { get; set; }

    public void Burn(int amount)
    {
        Total -= amount;
        BurnAction?.Invoke(amount);
    }

    public void Gain(int amount)
    {
        Total += amount;
        GainAction?.Invoke(amount);
    }
}