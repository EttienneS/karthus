public class WhiteMana
{
    public static void BurnWhite(float amount)
    {
    }

    public static void GainWhite(float amount)
    {
    }

    public static Mana GetBase(float startingTotal = 0)
    {
        return new Mana(ManaColor.White, GainWhite, BurnWhite)
        {
            Total = startingTotal,
        };
    }
}