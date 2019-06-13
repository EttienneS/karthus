public class RedMana
{
    public static void BurnRed(int amount)
    {
    }

    public static void GainRed(int amount)
    {
    }

    public static Mana GetBase(int startingTotal = 0)
    {
        return new Mana(ManaColor.Red, GainRed, BurnRed)
        {
            Total = startingTotal,
        };
    }
}