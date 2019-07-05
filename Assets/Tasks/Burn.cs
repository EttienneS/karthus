using System.Collections.Generic;

public class Burn : TaskBase
{
    public Burn()
    {
    }

    public Dictionary<ManaColor, int> ManaToBurn;
    public int AmountToChannel;
    public string TargetId;

    public Burn(Dictionary<ManaColor, int> castKvp, string targetId)
    {
        ManaToBurn = castKvp;
        TargetId = targetId;

        AddSubTask(new Move(Game.MapGrid.GetPathableNeighbour(IdService.GetLocation(targetId))));
    }

    public override bool Done()
    {
        if (Faction.QueueComplete(SubTasks))
        {
            foreach (var kvp in ManaToBurn)
            {
                if (kvp.Value > 0)
                {
                    var msg = $"{kvp.Key}!!";
                    AddSubTask(new Wait(1, msg, true));
                    ManaToBurn[kvp.Key]--;

                    Creature.BurnMana(kvp.Key);

                    switch (IdService.GetObjectTypeForId(TargetId))
                    {
                        case IdService.ObjectType.Creature:
                            IdService.GetCreatureFromId(TargetId).ManaPool[kvp.Key].Gain(1);
                            break;

                        case IdService.ObjectType.Structure:
                            IdService.GetStructureFromId(TargetId).ManaPool[kvp.Key].Gain(1);
                            break;
                    }

                    return false;
                }
            }
            return true;
        }

        return false;
    }
}