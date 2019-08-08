public class Spawn : SpellBase
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
                                                                 Originator.Coordinates,
                                                                 Originator.GetFaction()));
        }

        return false;
    }
}