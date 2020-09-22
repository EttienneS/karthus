using Assets.ServiceLocator;
using Assets.Structures;
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
        public OrderDetailItem OrderDetailPrefab;
        public GameObject CurrentOrdersPanel;
        public GameObject OrderOptionsPanel;
        public GameObject WorkDetailPanel;
        public Text StructureInfo;
        public Text Title;
        public WorkOrderPrefab WorkOrderPrefab;

        internal List<WorkOrderPrefab> ActivePrefabs;
        internal List<OrderDetailItem> DetailItems = new List<OrderDetailItem>();
        internal WorkOrderPrefab Selected;

        public void Add()
        {
            foreach (var structure in Structures.OfType<WorkStructureBase>())
            {
                structure.AddWorkOrder(1, Selected.Option);
            }
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public void Remove()
        {
            foreach (var structure in Structures)
            {
                if (!Loc.GetFactionController().PlayerFaction.AvailableTasks.OfType<RemoveStructure>().Any(r => r.StructureToRemove == structure))
                {
                    Loc.GetFactionController().PlayerFaction.AddTask(new RemoveStructure(structure));
                }
                else
                {
                    Debug.Log("Already added task to remove");
                }
            }
        }

        public List<Structure> Structures { get; set; }

        public void Show(List<Structure> structures)
        {
            Structures = structures;
            ResetPanel();

            WorkDetailPanel.SetActive(false);
            if (structures[0] is WorkStructureBase workStructure && structures.All(s => s.Name == structures[0].Name))
            {
                ActivePrefabs = new List<WorkOrderPrefab>();

                foreach (var option in workStructure.Definition.Options)
                {
                    var prefab = Instantiate(WorkOrderPrefab, OrderOptionsPanel.transform);
                    prefab.Load(workStructure, workStructure.Definition, option, this);
                    ActivePrefabs.Add(prefab);
                }

                if (ActivePrefabs.Count > 0)
                {
                    SetSelected(ActivePrefabs[0]);
                    if (workStructure.Definition.Auto)
                    {
                        AddButton.GetComponentInChildren<Text>().text = "Select";
                    }
                }

                WorkDetailPanel.SetActive(true);
            }
        }

        public void Claim()
        {
            foreach (var structure in Structures)
            {
                structure.FactionName = Loc.GetFactionController().PlayerFaction.FactionName;
                if (!Loc.GetFactionController().PlayerFaction.Structures.Contains(structure))
                {
                    Loc.GetFactionController().PlayerFaction.Structures.Add(structure);
                }
            }
        }

        public void Update()
        {
            var Current = Structures[0];
            StructureInfo.text = string.Empty;

            if (!Structures.All(s => s.Name == Structures[0].Name))
            {
                Title.text = $"Various Structures ({Structures.Count})";
            }
            else
            {
                Title.text = $"{Current.Name}";

                if (Current.InUseByAnyone)
                {
                    StructureInfo.text += $"In use by:\t{Current.InUseBy.Name}\n";
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
                            var detailItem = Instantiate(OrderDetailPrefab, CurrentOrdersPanel.transform);
                            detailItem.Load(order);
                            DetailItems.Add(detailItem);
                        }
                    }
                }
            }
        }

        internal void SetSelected(WorkOrderPrefab selected)
        {
            foreach (var prefab in ActivePrefabs)
            {
                prefab.Background.color = ColorConstants.WhiteBase;
            }
            selected.Background.color = ColorConstants.GreenBase;

            Selected = selected;
        }

        private void ResetPanel()
        {
            foreach (Transform detailItem in CurrentOrdersPanel.transform)
            {
                Destroy(detailItem.gameObject);
            }
            foreach (Transform orderItem in OrderOptionsPanel.transform)
            {
                Destroy(orderItem.gameObject);
            }
            DetailItems.Clear();
        }
    }
}