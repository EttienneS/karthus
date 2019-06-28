public class Store : BaseRune
{
    public int Capacity;

    public override bool Done()
    {
        if (RuneStructure.ManaPool.ManaCount() > Capacity)
        {
            RuneStructure.ManaPool.BurnMana(RuneStructure.ManaPool.GetRandomManaColorFromPool(), 1);
        }

        return base.Done();
    }
}