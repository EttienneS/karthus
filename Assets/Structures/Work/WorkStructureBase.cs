using System;
using System.Collections.Generic;
using System.Linq;

namespace Structures.Work
{
    public abstract class WorkStructureBase : Structure
    {
        public List<WorkOrderBase> Orders { get; set; } = new List<WorkOrderBase>();

        public WorkDefinition Definition { get; set; }

        public void AddWorkOrder(int amount, WorkOption option)
        {
            var orderBase = GetOrder(option, amount);
            orderBase.Amount = amount;

            Faction.AddTask(new DoWork(this, orderBase));
            Orders.Add(orderBase);
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

            Update(delta);
        }

        public abstract void Update(float delta);
    }
}