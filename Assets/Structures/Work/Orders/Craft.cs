using System.Linq;

namespace Structures.Work.Orders
{
    public class Craft : WorkOrderBase
    {
        public override void OrderComplete()
        {
            Complete = true;
        }

        public override void UnitComplete(float quality)
        {
            foreach (var cost in Option.Cost.Items)
            {
                var totalNeeded = cost.Value;
                foreach (var item in Structure.Cell.Items.Where(i => i.IsType(cost.Key)).ToList())
                {
                    if (totalNeeded <= 0)
                    {
                        break;
                    }

                    if (item.Amount >= totalNeeded)
                    {
                        item.Amount -= totalNeeded;
                        totalNeeded = 0;
                    }
                    else
                    {
                        totalNeeded -= item.Amount;
                        item.Amount = 0;
                    }

                    if (item.Amount == 0)
                    {
                        Game.Instance.ItemController.DestroyItem(item);
                    }
                }
            }

            Game.Instance.ItemController.SpawnItem(Option.Name, Structure.GetOutputCell(), Option.Amount);
        }
    }
}