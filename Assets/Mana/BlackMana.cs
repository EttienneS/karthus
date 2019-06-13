public class BlackMana
{
    public static void BurnBlack(int amount)
    {
    }

    public static void GainBlack(int amount)
    {
    }

    public static Mana GetBase(int startingTotal = 0)
    {
        return new Mana(ManaColor.Black, GainBlack, BurnBlack)
        {
            Total = startingTotal,
        };
    }
}