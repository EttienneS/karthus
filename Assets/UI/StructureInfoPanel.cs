using Structures;
using Structures.Work;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StructureInfoPanel : MonoBehaviour
    {
        public Button AddButton;
        public Structure Current;
        public OrderDetailItem OrderDetailPrefab;
        public GameObject OrderItemListPanel;
        public GameObject OrderPanel;
        public Text StructureInfo;
        public Text Title;
        public WorkOrderPrefab WorkOrderPrefab;

        internal List<WorkOrderPrefab> ActivePrefabs;
        internal List<OrderDetailItem> DetailItems = new List<OrderDetailItem>();
        internal WorkOrderPrefab Selected;

        public void Add()
        {
            Selected.Structure.AddWorkOrder(1, Selected.Option);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show(IEnumerable<Structure> entities)
        {
            gameObject.SetActive(true);

            var structure = entities.First();
            Current = structure;
            ResetPanel();

            AddButton.gameObject.SetActive(false);
            if (structure is WorkStructureBase workStructure)
            {
                ActivePrefabs = new List<WorkOrderPrefab>();

                foreach (var option in workStructure.Definition.Options)
                {
                    var prefab = Instantiate(WorkOrderPrefab, OrderPanel.transform);
                    prefab.Load(workStructure, workStructure.Definition, option);
                    ActivePrefabs.Add(prefab);
                }

                if (ActivePrefabs.Count > 0)
                {
                    SetSelected(ActivePrefabs[0]);

                    AddButton.gameObject.SetActive(true);
                    if (workStructure.Definition.Auto)
                    {
                        AddButton.GetComponentInChildren<Text>().text = "Select";
                    }
                }
            }
        }

        public void Start()
        {
            AddButton.enabled = false;
        }

        public void Update()
        {
            Title.text = $"{Current.Name}";

            StructureInfo.text = string.Empty;

            if (Current.InUseByAnyone)
            {
                StructureInfo.text += $"In use by:\t{Current.InUseBy.Name}\n";
            }
            if (Current.IsBluePrint)
            {
                StructureInfo.text += "\n** Blueprint, waiting for construction... **\n";
            }
            else if (Current is Container container)
            {
                StructureInfo.text += $"\nContainer:\n";

                if (container.Empty)
                {
                    StructureInfo.text += $"Contains:\tNothing\n";
                }
                else
                {
                    StructureInfo.text += $"Contains:\t{container.ItemType}\n";
                }

                StructureInfo.text += $"Capacity:\t{container.Count}/{container.Capacity}\n";
                StructureInfo.text += $"Filter:\t{container.Filter}\n";
            }
            else if (Current is WorkStructureBase workStructure)
            {
                StructureInfo.text += workStructure.ToString();

                if (workStructure.AutoCooldown > 0)
                {
                    StructureInfo.text += $"Next {workStructure.AutoOrder.Name} in {workStructure.AutoCooldown:0,0}/{workStructure.Definition.AutoCooldown:0,0}";
                }

                foreach (var order in DetailItems.ToList())
                {
                    if (order.WorkOrder.Complete)
                    {
                        DetailItems.Remove(order);
                        Destroy(order.gameObject);
                    }
                }

                foreach (var order in workStructure.Orders)
                {
                    if (!DetailItems.Any(d => d.WorkOrder == order))
                    {
                        var detailItem = Instantiate(OrderDetailPrefab, OrderItemListPanel.transform);
                        detailItem.Load(order);
                        DetailItems.Add(detailItem);
                    }
                }
            }
        }

        internal void SetSelected(WorkOrderPrefab selected)
        {
            foreach (var prefab in ActivePrefabs)
            {
                prefab.Background.color = Color.white;
            }
            selected.Background.color = Color.green;

            Selected = selected;
            AddButton.enabled = true;
        }

        private void ResetPanel()
        {
            foreach (Transform detailItem in OrderItemListPanel.transform)
            {
                Destroy(detailItem.gameObject);
            }
            foreach (Transform orderItem in OrderPanel.transform)
            {
                Destroy(orderItem.gameObject);
            }
            DetailItems.Clear();
        }
    }
}