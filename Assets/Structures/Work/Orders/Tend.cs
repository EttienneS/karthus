namespace Structures.Work.Orders
{
    public class Tend : WorkOrderBase
    {
        public override void OrderComplete()
        {
            (StructureId.GetEntity() as WorkStructureBase).AddWorkOrder(1, Option);
            Complete = true;
        }

        public override void UnitComplete(float quality)
        {
            var farmPlot = StructureId.GetEntity() as Farm;

            if (farmPlot.PlantName != Option.Name)
            {
                farmPlot.PlantName = Option.Name;
                farmPlot.CurrentGrowth = 0;
                farmPlot.Quality = 0;
            }

            farmPlot.Quality += quality;
        }
    }
}