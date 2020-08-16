using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

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

        public bool HasMaterial()
        {
            return GetRequiredMaterial().Count == 0;
        }

        internal Dictionary<string, int> GetRequiredMaterial()
        {
            var missingItems = new Dictionary<string, int>();
            if (Option.Cost == null)
            {
                return missingItems;
            }

            var itemsInCell = Structure.Cell.Items;
            foreach (var costItem in Option.Cost.Items)
            {
                var amountNeeded = costItem.Value;
                foreach (var itemInCell in itemsInCell.Where(i => i.IsType(costItem.Key)))
                {
                    amountNeeded -= itemInCell.Amount;
                }

                if (amountNeeded > 0)
                {
                    missingItems.Add(costItem.Key, amountNeeded);
                }
            }
            return missingItems;
        }
    }
}