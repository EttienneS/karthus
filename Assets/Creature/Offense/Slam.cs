using UnityEngine;

public class Strike : OffensiveActionBase
{
    public int DiceSize { get; set; }

    public Strike(string name, int diceSize, DamageType damageType) : base(name, damageType, activationTime: 1, range: 1)
    {
        DiceSize = diceSize;
    }

    public override int Damage
    {
        get
        {
            return Random.Range(1, DiceSize) + Owner.STR;
        }
    }
}