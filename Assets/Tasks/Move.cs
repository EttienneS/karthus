using Newtonsoft.Json;
using Assets.Creature;

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

    public override bool Done(CreatureData creature)
    {
        if (creature.UnableToFindPath)
        {
            // clear unable to find variable to ensure we can move again
            creature.UnableToFindPath = false; 
            throw new TaskFailedException("Unable to find path, cancel move.");
        }

        if (!creature.TargetCoordinate.x.AlmostEquals(TargetX) || !creature.TargetCoordinate.z.AlmostEquals(TargetZ))
        {
            creature.SetTargetCoordinate(TargetX, TargetZ);
        }
       
        if (creature.X == TargetX && creature.Z == TargetZ)
        {
            // dynamic map expansion
            // Game.Instance.Map.ExpandChunksAround(creature.Cell);
            creature.SetAnimation(AnimationType.Idle);
            return true;
        }
        return false;
    }
}