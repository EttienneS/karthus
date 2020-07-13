using Assets.Structures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Structures.Work
{
    public abstract class WorkStructureBase : Structure
    {
        public List<WorkOrderBase> Orders { get; set; } = new List<WorkOrderBase>();

        public WorkDefinition Definition { get; set; }

        public WorkOrderBase AutoOrder { get; set; }

        public float AutoCooldown { get; set; }

        public void AddWorkOrder(int amount, WorkOption option)
        {
            var orderBase = GetOrder(option, amount);
            orderBase.Amount = amount;

            if (Definition.Auto)
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
            else
            {
                Orders.Add(orderBase);
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
    }
}