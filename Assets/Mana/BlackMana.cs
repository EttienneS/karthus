public class BlackMana
{
    public static void BurnBlack(float amount)
    {
    }

    public static void GainBlack(float amount)
    {
    }

    public static Mana GetBase(float startingTotal = 0)
    {
        return new Mana(ManaColor.Black, GainBlack, BurnBlack)
        {
            Total = startingTotal,
        };
    }
}