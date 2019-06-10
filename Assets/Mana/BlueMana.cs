public class BlueMana
{
    public static void BurnBlue(int amount)
    {
    }

    public static void GainBlue(int amount)
    {
    }

    public static Mana GetBase(int startingTotal = 0)
    {
        return new Mana(ManaColor.Blue, GainBlue, BurnBlue)
        {
            Total = startingTotal,
        };
    }
}