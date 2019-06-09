public class Eat : TaskBase
{
    public Eat()
    {
    }

    public Eat(string itemCategory)
    {
        AddSubTask(new Wait(2f, "Eating"));
        Message = $"Eating Green";
    }

    public override bool Done()
    {
        if (Faction.QueueComplete(SubTasks))
        {
            FactionManager.Factions[Creature.Faction].Mana[ManaColor.Green].Burn(1);
            Creature.ValueProperties[Prop.Hunger] -= 50;
            return true;
        }

        return false;
    }
}