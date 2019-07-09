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
        if (Faction.QueueComplete(SubTasks))
        {
            FireRune(() => Game.CreatureController.SpawnCreature(Game.CreatureController.GetCreatureOfType("Person"),
                                                                 IdService.GetLocation(Originator),
                                                                 IdService.GetFactionForId(Originator)));
        }

        return false;
    }
}