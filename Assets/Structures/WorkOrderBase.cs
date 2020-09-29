using Assets.ServiceLocator;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Structures
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
                    _structure = StructureId.GetStructure() as WorkStructureBase;
                }
                return _structure;
            }
        }

        public string StructureId { get; set; }

        public bool Active()
        {
            return Structure.Orders.Contains(this);
        }

        public bool HasMaterial()
        {
            return GetRequiredMaterial().Count == 0;
        }

        public abstract void OrderComplete();

        public abstract void UnitComplete(float quality);

        internal void ConsumeCostItems()
        {
            foreach (var cost in Option.Cost.Items)
            {
                var totalNeeded = cost.Value;
                foreach (var item in Structure.Cell.Items.Where(i => i.IsType(cost.Key)).ToList())
                {
                    if (totalNeeded <= 0)
                    {
                        break;
                    }

                    if (item.Amount >= totalNeeded)
                    {
                        item.Amount -= totalNeeded;
                        totalNeeded = 0;
                    }
                    else
                    {
                        totalNeeded -= item.Amount;
                        item.Amount = 0;
                    }

                    if (item.Amount == 0)
                    {
                        Loc.GetItemController().DestroyItem(item);
                    }
                }
            }
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