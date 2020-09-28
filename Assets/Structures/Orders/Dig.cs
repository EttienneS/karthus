using Assets.ServiceLocator;
using Assets.Structures;

namespace Assets.Structures.Orders
{
    public class Dig : WorkOrderBase
    {
        public override void OrderComplete()
        {
            Complete = true;
        }

        public override void UnitComplete(float quality)
        {
            if (!Active())
            {
                return;
            }
            Loc.GetItemController().SpawnItem(Option.Name, Structure.Cell, 1);
        }
    }
}