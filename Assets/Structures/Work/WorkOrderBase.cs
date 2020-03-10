using Newtonsoft.Json;

namespace Structures.Work
{
    public abstract class WorkOrderBase
    {
        private WorkStructureBase _structure;
        public int Amount { get; set; }
        public bool Complete { get; set; }
        public string Name { get; set; }
        public WorkOption Option { get; set; }
        public float Progress { get; set; }
        public string Skill { get; set; }

        [JsonIgnore]
        public WorkStructureBase Structure
        {
            get
            {
                if (_structure == null)
                {
                    _structure = StructureId.GetEntity() as WorkStructureBase;
                }
                return _structure;
            }
        }

        public string StructureId { get; set; }

        public bool Active()
        {
            return Structure.Orders.Contains(this);
        }

        public abstract void OrderComplete();

        public abstract void UnitComplete(float quality);
    }
}