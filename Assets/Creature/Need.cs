public class Need
{
    public float Current { get; set; }
    public float Max { get; set; }
    public string Name { get; set; }

    public Need()
    {
    }

    public Need(string name, float max, float current) : base()
    {
        Name = name;
        Max = max;
        Current = current;
    }

    public override string ToString()
    {
        return $"{Name} [{Current:0.0}/{Max}]";
    }
}
public static class NeedNames
{
    public const string Hunger = "Hunger";
    public const string Joy = "Joy";
    public const string Energy = "Energy";
    public const string Comfort = "Comfort";
    public const string Hygiene = "Hygiene";
    public const string Social = "Social";
    public const string Beauty = "Beauty";
    public const string Aspiration = "Aspiration";
}