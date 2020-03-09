namespace Structures.Work
{
    public class WorkDefinition
    {
        public string Name { get; set; }

        public string RequiredSkillName { get; set; }

        public WorkOption[] Options { get; set; }

        public string WorkOrderType { get; set; }

        public bool Auto { get; set; }
    }
}