public class Feeling
{
    public string Description { get; set; }
    public float DurationLeft { get; set; }
    public string Icon { get; set; }
    public int MoodImpact { get; set; }
    public string Name { get; set; }
    public string Source { get; set; }
    public string[] Tags { get; set; }

    public Feeling(string name, int impact, float duration)
    {
        Name = name;
        MoodImpact = impact;
        DurationLeft = duration;
    }

    public override string ToString()
    {
        var sign = MoodImpact > 0 ? "+" : "";
        return $"{Name}: {Description} [{sign}{MoodImpact}] {DurationLeft}";
    }
}