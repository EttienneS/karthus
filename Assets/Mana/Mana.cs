using Newtonsoft.Json;
using System;

public class Mana
{
    public Mana(ManaColor color, Action<float> gainAction, Action<float> burnAction)
    {
        GainAction = gainAction;
        BurnAction = burnAction;
        Color = color;
    }

    [JsonIgnore]
    public Action<float> BurnAction { get; set; }

    public ManaColor Color { get; set; }
    public int Desired { get; set; }

    [JsonIgnore]
    public Action<float> GainAction { get; set; }

    public float Total { get; set; }
    public float Attunement { get; set; }

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

    internal bool OverAttuned()
    {
        return Total - Attunement >= 1f;
    }

    internal bool OverDesired()
    {
        return Total - Desired >= 1f;
    }

    internal bool UnderDesired()
    {
        return Total - Desired < 0f;
    }

    internal bool Unbalanced()
    {
        return OverAttuned() || OverDesired() || UnderDesired();
    }
}