using Assets.Creature;
using Assets.Structures;
using Needs;
using Newtonsoft.Json;

public class Sleep : CreatureTask
{
    public string BedId;
    public float RecoveryRate = 0.1f;
    public string CreatureID;

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

        OnSuspended += () => ResetCreatureState(CreatureID.GetCreature());
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

    public override void FinalizeTask()
    {
        var bed = Bed;

        if (bed != null)
        {
            bed.Free();
        }
    }

    public override bool Done(CreatureData creature)
    {
        CreatureID = creature.Id;
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
            creature.GetNeed<Hunger>().CurrentChangeRate = NeedConstants.BaseDegrateRate / 2;
            creature.SetAnimation(AnimationType.Sleeping);
            if (creature.GetCurrentNeed<Energy>() > 90f)
            {
                ResetCreatureState(creature);
                return true;
            }
        }

        return false;
    }

    private static void ResetCreatureState(CreatureData creature)
    {
        if (creature != null)
        {
            creature.SetAnimation(AnimationType.Idle);
            creature.GetNeed<Energy>().ResetRate();
            creature.GetNeed<Hunger>().ResetRate();
        }
    }
}