public class Eat : TaskBase
{
    public Eat()
    {
    }

    public ManaColor FoodColor;

    public Eat(ManaColor foodColor)
    {
        FoodColor = foodColor;
        AddSubTask(new Wait(2f, $"Eating {foodColor}"));
    }

    public override bool Done()
    {
        if (Faction.QueueComplete(SubTasks))
        {
            Creature.Faction.Structure.ManaPool[FoodColor].Burn(1);
            Creature.GainMana(FoodColor);
            Creature.BurnMana(FoodColor);
            Creature.ValueProperties[Prop.Hunger] -= 50;
            return true;
        }

        return false;
    }
}