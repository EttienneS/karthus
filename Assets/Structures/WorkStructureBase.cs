using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Structures
{
    public abstract class WorkStructureBase : Structure
    {
        public delegate bool CanPlaceOrderDelegate();

        public float AutoCooldown { get; set; }
        public WorkOrderBase AutoOrder { get; set; }

        [JsonIgnore]
        public CanPlaceOrderDelegate CanPlaceOrder { get; set; }

        public WorkDefinition Definition { get; set; }
        public List<WorkOrderBase> Orders { get; set; } = new List<WorkOrderBase>();

        public void AddWorkOrder(int amount, WorkOption option)
        {
            var orderBase = GetOrder(option, amount);
            orderBase.Amount = amount;

            switch (Definition.OrderTrigger)
            {
                case OrderTrigger.AutoCondition:
                case OrderTrigger.Auto:
                    PlaceAutoOrder(orderBase);
                    break;

                case OrderTrigger.Manual:
                    Orders.Add(orderBase);
                    break;

            }
        }

        private void PlaceAutoOrder(WorkOrderBase orderBase)
        {
            Orders.ForEach(o => o.Complete = true);
            Orders.Clear();
            AutoOrder = orderBase;
            AutoCooldown = Definition.AutoCooldown;

            if (Definition.SkipInitialDelay)
            {
                // first time it should immediately run
                Definition.SkipInitialDelay = false;
                AutoCooldown = 0.00000001f;
            }
        }

        public WorkOrderBase GetOrder(WorkOption option, int amount)
        {
            var type = WorkHelper.GetTypeFor(Definition.WorkOrderType);
            var order = Activator.CreateInstance(type, null) as WorkOrderBase;
            order.Name = $"{Definition.Name} : {option.Name}";
            order.Option = option;
            order.Amount = amount;
            order.Skill = Definition.RequiredSkillName;
            order.StructureId = Id;

            return order;
        }

        public abstract void Initialize();

        public void Process(float delta)
        {
            foreach (var order in Orders.ToList())
            {
                if (order.Complete)
                {
                    Orders.Remove(order);
                }
            }

            if (InUseByAnyone)
            {
                return;
            }

            if (AutoOrder != null)
            {
                if (AutoCooldown > 0)
                {
                    AutoCooldown -= delta;
                }

                if (Definition.OrderTrigger == OrderTrigger.AutoCondition && !CanPlaceOrder.Invoke())
                {
                    // cant trigger yet, wait and then try again
                    AutoCooldown = Definition.AutoCooldown / 2;
                }

                if (AutoCooldown <= 0)
                {
                    Faction.AddTask(new DoWork(this, AutoOrder));
                    Orders.Add(AutoOrder);
                    AutoOrder = null;
                }
            }
            else
            {
                var order = Orders.FirstOrDefault();

                var tasks = Faction.AvailableTasks.OfType<DoWork>().ToList();
                tasks.AddRange(Faction.Creatures.Select(s => s.Task).OfType<DoWork>().ToList());
                if (order != null && !tasks.Any(t => t.Order == order))
                {
                    Faction.AddTask(new DoWork(this, order));
                }
            }

            Update(delta);
        }

        public abstract void Update(float delta);

        internal Cell GetOutputCell()
        {
            var workcell = GetWorkCell();
            return Cell.NonNullNeighbors.First(c => c != workcell && c.PathableWith(Mobility.Walk));
        }

        internal Cell GetWorkCell()
        {
            return Cell.NonNullNeighbors.First(c => c.PathableWith(Mobility.Walk));
        }

        internal void PlaceDefaultOrder()
        {
            var defaultOrder = Definition?.Options?.FirstOrDefault(o => o.Default);
            if (defaultOrder != null)
            {
                AddWorkOrder(1, defaultOrder);
            }
        }
    }
}