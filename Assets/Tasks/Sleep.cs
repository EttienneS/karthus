using Needs;
using Newtonsoft.Json;
using Assets.Creature;
using Assets.Structures;

public class Sleep : CreatureTask
{
    public string BedId;
    public float RecoveryRate = 1.25f;

    public override string Message
    {
        get
        {
            return $"Sleep at {BedId}";
        }
    }

    public Sleep()
    {
        BusyEmote = "Zzzz...";
    }

    [JsonIgnore]
    public Structure Bed
    {
        get
        {
            if (string.IsNullOrEmpty(BedId))
            {
                return null;
            }
            return BedId.GetStructure();
        }
    }

    public bool Sleeping { get; set; }

    public override void Complete()
    {
        var bed = Bed;

        if (bed != null)
        {
            bed.Free();
        }
    }

    public override bool Done(CreatureData creature)
    {
        if (SubTasksComplete(creature))
        {
            if (string.IsNullOrEmpty(BedId) && !Sleeping)
            {
                var bed = creature.Faction.Structures.Find(s => !s.InUseByAnyone && s.ValueProperties.ContainsKey("RecoveryRate"));

                if (bed != null)
                {
                    bed.Reserve(creature);
                    AddSubTask(new Move(bed.Cell));
                    BedId = bed.Id;
                    return false;
                }
            }
            Sleeping = true;
            creature.GetNeed<Energy>().CurrentChangeRate = RecoveryRate;

            if (creature.GetCurrentNeed<Energy>() > 90f)
            {
                creature.GetNeed<Energy>().ResetRate();
                return true;
            }
        }

        return false;
    }
}