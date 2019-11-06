using System;
using System.Collections.Generic;
using System.Linq;

public class Block : DefensiveActionBase
{
    public int Defense { get; set; }

    public List<DamageType> BlockableTypes = new List<DamageType>();

    public Block(string name, int defense, params DamageType[] blockableTypes) : base(name)
    {
        Defense = defense;
        ActivationTIme = 0.5f;
        BlockableTypes = blockableTypes.ToList();
    }

    public override int Defend(string attackName, int incomingDamage, DamageType damageType)
    {
        if (BlockableTypes.Contains(damageType))
        {
            var damageAfterBlock = Math.Max(0, incomingDamage - Defense);
            LogBlock(attackName, incomingDamage, damageAfterBlock);

            Limb.Damage(damageType, Defense / 2);
            return damageAfterBlock;
        }
        else
        {
            var damageAfterBlock = Math.Max(0, incomingDamage - (Defense / 2));
            LogBlock(attackName, incomingDamage, damageAfterBlock);

            Limb.Owner.Log($"{Limb.Name} is damaged greatly by the {attackName}");
            Limb.Damage(damageType, Defense * 2);

            return damageAfterBlock;
        }
    }

    private void LogBlock(string attackName, int incomingDamage, int damageAfterBlock)
    {
        if (damageAfterBlock == 0)
        {
            Limb.Owner.Log($"All {attackName}'s damage [{incomingDamage}] is blocked by {Owner.Name}'s {Name}");
        }
        else
        {
            Limb.Owner.Log($"Some of the {attackName}'s damage [{incomingDamage - damageAfterBlock}] is blocked by {Owner.Name}'s {Name}");
        }
    }

    public override int EstimateDefense(OffensiveActionBase offensiveActionBase)
    {
        if (BlockableTypes.Contains(offensiveActionBase.DamageType))
        {
            return offensiveActionBase.PredictDamage(Owner) - Defense;
        }
        else
        {
            return offensiveActionBase.PredictDamage(Owner) - (Defense / 2);
        }
    }
}