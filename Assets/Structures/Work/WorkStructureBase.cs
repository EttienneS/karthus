using System;
using System.Collections.Generic;

namespace Structures.Work
{
    public abstract class WorkStructureBase : Structure
    {
        public List<WorkOrderBase> Orders { get; set; } = new List<WorkOrderBase>();
        public int SelectedDefinition { get; set; }
        public int SelectedOption { get; set; }
        public WorkDefinition[] WorkDefinitions { get; set; }

        public void AddWorkOrder(int amount)
        {
            var def = WorkDefinitions[SelectedDefinition];
            var opt = def.Options[SelectedOption];

            var orderBase = GetOrder(def, opt, amount);
            orderBase.Amount = amount;

            Faction.AddTask(new DoWork(this, orderBase));
            Orders.Add(orderBase);
        }

        public WorkOrderBase GetOrder(WorkDefinition definition, WorkOption option, int amount)
        {
            var type = WorkHelper.GetTypeFor(definition.WorkOrderType);
            var order = Activator.CreateInstance(type, null) as WorkOrderBase;
            order.Name = $"{definition.Name} : {option.Name}";
            order.Option = option;
            order.Amount = amount;
            order.Skill = definition.RequiredSkillName;
            order.StructureId = Id;

            return order;
        }
        public abstract void Update(float delta);
    }
}