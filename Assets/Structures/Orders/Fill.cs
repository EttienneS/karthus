namespace Assets.Structures.Orders
{
    public class Fill : WorkOrderBase
    {
        public override void OrderComplete()
        {
            Structure.AddWorkOrder(1, Option);
            Complete = true;
        }

        public override void UnitComplete(float quality)
        {
            ConsumeCostItems();

            var barrel = StructureId.GetStructure() as LiquidContainer;
            barrel.FillLevel += 25;
        }
    }
}