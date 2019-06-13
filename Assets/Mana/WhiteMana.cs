public class WhiteMana
{
    public static void BurnWhite(int amount)
    {
    }

    public static void GainWhite(int amount)
    {
    }

    public static Mana GetBase(int startingTotal = 0)
    {
        return new Mana(ManaColor.White, GainWhite, BurnWhite)
        {
            Total = startingTotal,
        };
    }
}