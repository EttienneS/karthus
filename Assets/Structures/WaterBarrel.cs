namespace Assets.Structures
{
    public class WaterBarrel : WorkStructureBase
    {
        public int Capacity { get; set; }
        public int FillLevel { get; set; }

        public override void Initialize()
        {
            CanPlaceOrder = ShouldRefill;
        }

        public bool ShouldRefill()
        {
            return FillLevel < 90;
        }

        public override void Update(float delta)
        {
        }

        public override string ToString()
        {
            return $"{Name}:\n" +
                   $"  Fill: {FillLevel}\\{Capacity}\n";
        }
    }
}