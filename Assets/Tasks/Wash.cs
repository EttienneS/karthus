using Needs;
using Animation = LPC.Spritesheet.Generator.Interfaces.Animation;
using Structures;
public class Wash : CreatureTask
{
    public float RecoveryRate = 0.25f;
    public (float X, float Y) WashCoords;

    public Wash()
    {
        BusyEmote = "**Scrub, scrub, scrub**";
    }

    public Wash(Structure bath) : this()
    {
        RecoveryRate = bath.GetValue("Hygiene");
        AddSubTask(new Move(bath.Cell.GetPathableNeighbour()));
        WashCoords = (bath.Cell.X, bath.Cell.Y);
    }

    public Wash(Cell cell) : this()
    {
        AddSubTask(new Move(cell.GetPathableNeighbour()));
        WashCoords = (cell.X, cell.Y);
    }


    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            var need = creature.GetNeed<Hygiene>();
            creature.SetAnimation(Animation.Thrust, 1);
            creature.Face(Game.Map.GetCellAtCoordinate(WashCoords));
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