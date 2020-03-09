namespace Structures.Work
{
    public class Farm : WorkStructureBase
    {
        public string PlantName { get; set; }
        public float CurrentGrowth { get; set; }
        public float MaxGrowth { get; set; } = 100;
        public float Quality { get; set; }

        public override void Update(float delta)
        {
            if (string.IsNullOrEmpty(PlantName) && Orders.Count == 0)
            {
                AddWorkOrder(1);
            }
            else if (CurrentGrowth >= MaxGrowth)
            {
                Game.ItemController.SpawnItem(PlantName, Cell, (int)Quality);

            }
        }
    }
}