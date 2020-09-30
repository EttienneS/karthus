namespace Assets.Structures.Orders
{
    public class Tend : WorkOrderBase
    {
        public override void OrderComplete()
        {
            if (Active())
            {
                Structure.AddWorkOrder(1, Option);
            }
            Complete = true;
        }

        public override void UnitComplete(float quality)
        {
            if (!Active())
            {
                return;
            }

            ConsumeCostItems();
            var farmPlot = StructureId.GetStructure() as Farm;

            // change to new plant
            if (farmPlot.PlantName != Option.Name)
            {
                farmPlot.PlantName = Option.Name;
                farmPlot.ResetGrowth();
            }
            else
            {
                if (farmPlot.IsMature())
                {
                    farmPlot.SpawnYield();
                    farmPlot.ResetGrowth();
                }
                else
                {
                    farmPlot.Quality += quality;
                }
            }
        }
    }
}