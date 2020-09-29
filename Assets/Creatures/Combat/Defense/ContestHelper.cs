public static class ContestHelper
{
    public static OutcomeLevel Contest(int attacker, int defender)
    {
        var diff = attacker - defender;
        if (diff > 10)
        {
            return OutcomeLevel.Critical;
        }
        else if (diff >= 0)
        {
            return OutcomeLevel.Success;
        }
        else if (diff < -10)
        {
            return OutcomeLevel.Catastrophe;
        }

        return OutcomeLevel.Failure;
    }
}