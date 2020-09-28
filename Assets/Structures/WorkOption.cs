namespace Assets.Structures
{
    public class WorkOption
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public float RequiredSkillLevel { get; set; }
        public float TimeToComplete { get; set; }
        public string Icon { get; set; }
        public int Amount { get; set; }
        public Cost Cost { get; set; }
    }
}