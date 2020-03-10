namespace Structures.Work.Orders
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

            var farmPlot = StructureId.GetEntity() as Farm;

            // change to new plant
            if (farmPlot.PlantName != Option.Name)
            {
                farmPlot.Sprite = Option.Sprite;
                farmPlot.PlantName = Option.Name;
                farmPlot.CurrentGrowth = 0;
                farmPlot.Quality = 0;
            }

            farmPlot.Quality += quality;
        }
    }
}