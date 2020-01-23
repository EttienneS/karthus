using Newtonsoft.Json;

public class Move : CreatureTask
{
    public float TargetX;
    public float TargetY;

    public Move()
    {
    }

    public Move(Cell targetCoordinates) : this()
    {
        TargetX = targetCoordinates.Vector.x;
        TargetY = targetCoordinates.Vector.y;

        Message = $"Moving to {TargetX}:{TargetY}";
    }

    [JsonIgnore]
    public Cell TargetCell
    {
        get
        {
            return Game.Map.GetCellAtCoordinate(TargetX, TargetY);
        }
    }

    public override bool Done(Creature creature)
    {
        if (creature.TargetCoordinate.x != TargetX || creature.TargetCoordinate.y != TargetY)
        {
            creature.SetTargetCoordinate(TargetX, TargetY);
        }
        if (creature.UnableToFindPath)
        {
            throw new TaskFailedException("Unable to find path");
        }
        return creature.X == TargetX && creature.Y == TargetY;
    }
}