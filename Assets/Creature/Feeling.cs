using Newtonsoft.Json;

public class Feeling
{
    public string Description { get; set; }
    public float DurationLeft { get; set; }
    public string Icon { get; set; }
    public int MoodImpact { get; set; }
    public string Name { get; set; }
    public string Source { get; set; }
    public string[] Tags { get; set; }


    public override string ToString()
    {
        var sign = MoodImpact > 0 ? "+" : "";
        return $"{Name}: {Description} [{sign}{MoodImpact}]";
    }
}