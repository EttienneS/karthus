using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Assets.Structures
{
    public class WorkDefinition
    {
        public string Name { get; set; }

        public string RequiredSkillName { get; set; }

        public WorkOption[] Options { get; set; }

        public string WorkOrderType { get; set; }

        public OrderTrigger OrderTrigger { get; set; }

        public float AutoCooldown { get; set; }

        public bool SkipInitialDelay { get; set; } = true;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderTrigger
    {
        Manual, Auto, AutoCondition
    }
}