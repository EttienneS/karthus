public class GreenMana
{
    public static void BurnGreen(float amount)
    {
    }

    public static void GainGreen(float amount)
    {
    }

    public static Mana GetBase(float startingTotal = 0)
    {
        return new Mana(ManaColor.Green, GainGreen, BurnGreen)
        {
            Total = startingTotal,
        };
    }
}