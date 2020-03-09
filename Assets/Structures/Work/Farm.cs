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
            if (!string.IsNullOrEmpty(PlantName))
            {
                CurrentGrowth += delta;
            }

            if (CurrentGrowth >= MaxGrowth)
            {
                Game.ItemController.SpawnItem(PlantName, Cell, (int)Quality);
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(PlantName))
            {
                return "\n** No crop selected. Choose one of the options and click the Select button. **";
            }

            return "Farm:\n" +
                   $"  Growing: {PlantName}\n" +
                   $"  Quality: {Quality}\n" +
                   $"  Grown: {(CurrentGrowth / MaxGrowth) * 100:0,0}%\n";
        }
    }
}