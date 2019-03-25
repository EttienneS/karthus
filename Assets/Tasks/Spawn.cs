public class Spawn : BaseRune
{
    public Spawn()
    {
    }

    public Spawn(float initialPower, float powerRate)
    {
        PowerRate = powerRate;
        Power = initialPower;
    }

    public override bool Done()
    {
        if (Taskmaster.QueueComplete(SubTasks))
        {
            FireRune(() => CreatureController.Instance.SpawnCreature(MapGrid.Instance.GetCellAtCoordinate(IdService.GetLocation(Originator))));
        }

        return false;
    }
}