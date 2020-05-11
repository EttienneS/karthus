using Newtonsoft.Json;

public class Move : CreatureTask
{
    public float TargetX;
    public float TargetZ;

    public override string Message
    {
        get
        {
            return $"Move to {TargetX}:{TargetZ}";
        }
    }

    public Move()
    {
    }

    public override void Complete()
    {
    }

    public Move(Cell targetCoordinates) : this()
    {
        TargetX = targetCoordinates.Vector.x;
        TargetZ = targetCoordinates.Vector.z;
    }

    [JsonIgnore]
    public Cell TargetCell
    {
        get
        {
            return Game.Instance.Map.GetCellAtCoordinate(TargetX, TargetZ);
        }
    }

    public override bool Done(Creature creature)
    {
        if (creature.TargetCoordinate.x != TargetX || creature.TargetCoordinate.z != TargetZ)
        {
            creature.SetTargetCoordinate(TargetX, TargetZ);
        }
        if (creature.UnableToFindPath)
        {
            throw new TaskFailedException("Unable to find path");
        }
        if (creature.X == TargetX && creature.Z == TargetZ)
        {
            // dynamic map expansion
            // Game.Instance.Map.ExpandChunksAround(creature.Cell);
            return true;
        }
        return false;
    }
}