namespace Structures.Work
{
    public abstract class WorkOrderBase
    {
        public string StructureId { get; set; }
        public string Name { get; set; }
        public float Progress { get; set; }
        public int Amount { get; set; }
        public WorkOption Option { get; set; }
        public string Skill { get; set; }

        public abstract void UnitComplete(float quality);

        public abstract void OrderComplete();
    }
}