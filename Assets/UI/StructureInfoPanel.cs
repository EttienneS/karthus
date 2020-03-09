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
        public WorkOrderPrefab WorkOrderPrefab;
        public OrderDetailItem OrderDetailPrefab;

        public Text Title;
        public Text StructureInfo;
        public GameObject OrderPanel;
        public Button AddButton;
        public GameObject OrderListPanel;


        internal List<WorkOrderPrefab> ActivePrefabs;

        public void Add()
        {
            if (Selected.Definition.Auto)
            {
                Selected.Structure.Orders.Clear();
            }
            Selected.Structure.AddWorkOrder(1, Selected.Option);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public Structure Current;

        public void Show(IEnumerable<Structure> entities)
        {
            gameObject.SetActive(true);

            var structure = entities.First();
            Current = structure;
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
                }

                if (workStructure.Definition.Auto)
                {
                    AddButton.GetComponentInChildren<Text>().text = "Select";
                }
            }
        }

        public void Update()
        {
            Title.text = Current.Name;

            StructureInfo.text = $"Rotation: {Current.Rotation}\n";

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

                foreach (var order in workStructure.Orders)
                {
                    Instantiate(OrderDetailPrefab, OrderListPanel.transform).Load(order);
                }
            }
        }

        internal WorkOrderPrefab Selected;

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

        public void Start()
        {
            AddButton.enabled = false;
        }
    }
}