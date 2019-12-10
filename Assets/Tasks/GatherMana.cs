using Newtonsoft.Json;
using UnityEngine;

public class GatherMana : CreatureTask
{
    public (int, int) Target;

    public GatherMana()
    {
        RequiredSkill = "Build";
        RequiredSkillLevel = 1;
    }

    public GatherMana(Cell cellToGather) : this()
    {
        Target = (cellToGather.X, cellToGather.Y);
    }
    [JsonIgnore]
    public Cell TargetCell
    {
        get
        {
            return Game.Map.CellLookup[Target];
        }
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            var target = TargetCell;
            if (creature.Cell != target)
            {
                AddSubTask(new Move(target));
            }
            else
            {
                var color = target.Liquid.Value;
                if (target.LiquidLevel > 0)
                {
                    var amount = Mathf.Min(target.LiquidLevel, 1f);

                    Game.VisualEffectController.SpawnLightEffect(null, creature.Vector, color.GetActualColor(), amount, amount, 3).Fades();
                    creature.ManaPool.GainMana(target.Liquid.Value, amount);
                    target.LiquidLevel -= amount;
                    target.UpdateLiquid();
                }
                else
                {
                    return true;
                }
            }
        }

        return false;
    }
}