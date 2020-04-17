using Structures.Work;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class OrderDetailItem : MonoBehaviour
    {
        public Text Title;

        internal WorkOrderBase WorkOrder;

        public void Load(WorkOrderBase workOrder)
        {
            WorkOrder = workOrder;
            Title.text = WorkOrder.Name;
        }

        public void OrderSelected()
        {

        }
    }
}