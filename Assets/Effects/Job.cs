public class Job : EffectBase
{
    public string[] Task { get; set; }

    private CreatureTask CurrentTask;

    public string GetTaskString()
    {
        var taskStr = string.Empty;

        foreach (var line in Task)
        {
            taskStr += line + "\n";
        }

        return taskStr.Replace("[THIS]", AssignedEntity.Id);
    }

    public override bool DoEffect()
    {
        var faction = AssignedEntity.GetFaction();
        if (CurrentTask?.Destroyed != false)
        {
            CurrentTask = GetTaskString().LoadJson<CreatureTask>();
            faction.AddTask(CurrentTask);
        }

        return true;
    }
}