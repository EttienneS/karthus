public class GreenMana
{
    public static void BurnGreen(int amount)
    {
    }

    public static void GainGreen(int amount)
    {
    }

    public static Mana GetBase(int startingTotal = 0)
    {
        return new Mana(ManaColor.Green, GainGreen, BurnGreen)
        {
            Total = startingTotal,
        };
    }
}