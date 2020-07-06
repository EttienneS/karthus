using Needs;
using Assets.Creature;
using Structures;
public class Wash : CreatureTask
{
    public float RecoveryRate = 0.25f;
    public (float X, float Y) WashCoords;

    public override string Message
    {
        get
        {
            return $"Wash at {WashCoords.X}:{WashCoords.Y}";
        }
    }

    public Wash()
    {
        BusyEmote = "**Scrub, scrub, scrub**";
    }

    public override void Complete()
    {
    }

    public Wash(Structure bath) : this()
    {
        RecoveryRate = bath.GetValue("Hygiene");
        AddSubTask(new Move(bath.Cell.GetPathableNeighbour()));
        WashCoords = (bath.Cell.X, bath.Cell.Z);
    }

    public Wash(Cell cell) : this()
    {
        AddSubTask(new Move(cell.GetPathableNeighbour()));
        WashCoords = (cell.X, cell.Z);
    }

    public override bool Done(CreatureData creature)
    {
        if (SubTasksComplete(creature))
        {
            var need = creature.GetNeed<Hygiene>();
            creature.Face(Game.Instance.Map.GetCellAtCoordinate(WashCoords));
            need.CurrentChangeRate = RecoveryRate;

            if (need.Current > 99f)
            {
                need.ResetRate();
                return true;
            }
        }

        return false;
    }
}