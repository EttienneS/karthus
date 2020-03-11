using Needs;
using Newtonsoft.Json;
using Structures;
using Animation = LPC.Spritesheet.Generator.Interfaces.Animation;

public class Sleep : CreatureTask
{
    public string BedId;
    public float RecoveryRate = 1.25f;

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

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            if (string.IsNullOrEmpty(BedId) && !Sleeping)
            {
                var bed = creature.Faction.Structures.Find(s => !s.IsBluePrint && !s.InUseByAnyone && s.ValueProperties.ContainsKey("RecoveryRate"));

                if (bed != null)
                {
                    bed.Reserve(creature);
                    AddSubTask(new Move(bed.Cell));
                    BedId = bed.Id;
                    return false;
                }
            }
            Sleeping = true;
            creature.SetFixedAnimation(Animation.Walk, 1);
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