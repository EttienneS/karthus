using Assets.ServiceLocator;
using Assets.Structures;
using System.Linq;

namespace Assets.Structures.Orders
{
    public class Craft : WorkOrderBase
    {
        public override void OrderComplete()
        {
            Complete = true;
        }

        public override void UnitComplete(float quality)
        {
            ConsumeCostItems();

            Loc.GetItemController().SpawnItem(Option.Name, Structure.GetOutputCell(), Option.Amount);
        }

        
    }
}