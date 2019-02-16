using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderSelectionController : MonoBehaviour
{
    public OrderButton OrderButtonPrefab;
    internal OrderButton TaskButton;
    internal OrderButton BuildButton;
    internal OrderButton StockpileButton;
    private static OrderSelectionController _instance;

    public delegate void CellClickedDelegate(List<Cell> cell);

    public static OrderSelectionController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("OrderPanel").GetComponent<OrderSelectionController>();
            }

            return _instance;
        }
    }

    public CellClickedDelegate CellClickOrder { get; set; }

    public void BuildClicked(string structureName)
    {
        BuildButton.Text = "Build " + structureName;
        GameController.Instance.SelectionPreference = SelectionPreference.CellOnly;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Data.Structure == null)
                {
                    var blueprint = StructureController.Instance.GetStructureBluePrint(structureName);
                    cell.AddContent(blueprint.gameObject);
                    Taskmaster.Instance.AddTask(new Build(blueprint.Data, cell.Data.Coordinates));
                }
            }
        };
    }

    public void BuildTypeClicked()
    {
        if (OrderTrayController.Instance.gameObject.activeInHierarchy)
        {
            DisableAndReset();

            BuildButton.Text = "Select Building";
        }
        else
        {
            EnableAndClear();

            foreach (var structureData in StructureController.Instance.StructureDataReference.Values)
            {
                if (!structureData.Buildable) continue;

                var button = Instantiate(OrderButtonPrefab, OrderTrayController.Instance.transform);
                button.Button.onClick.AddListener(() => BuildClicked(structureData.Name));
                button.name = structureData.Name;
                button.Button.image.sprite = StructureController.Instance.GetSpriteForStructure(structureData.Name);

                if (structureData.Tiled)
                {
                    button.Button.image.type = Image.Type.Tiled;
                }

                button.Text = "Build " + structureData.Name;
            }

            var removeButton = Instantiate(OrderButtonPrefab, OrderTrayController.Instance.transform);
            removeButton.Button.onClick.AddListener(RemoveStructureClicked);
            removeButton.name = "Remove Structure";
            removeButton.Text = removeButton.name;
            removeButton.Button.image.sprite = SpriteStore.Instance.GetSpriteByName("cancel");
        }
    }


    public void DesignateTypeClicked()
    {
        if (OrderTrayController.Instance.gameObject.activeInHierarchy)
        {
            DisableAndReset();
            TaskButton.Text = "Designate";
        }
        else
        {
            EnableAndClear();

            var cutButton = Instantiate(OrderButtonPrefab, OrderTrayController.Instance.transform);
            cutButton.Button.onClick.AddListener(CutTreeClicked);
            cutButton.name = "Cut Tree";
            cutButton.Text = cutButton.name;
            cutButton.Button.image.sprite = SpriteStore.Instance.GetSpriteByName("axe");
        }
    }


    private void CutTreeClicked()
    {
        BuildButton.Text = "Cut Tree";
        GameController.Instance.SelectionPreference = SelectionPreference.CellOnly;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Data.Structure != null)
                {
                    var structure = cell.Data.Structure;
                    structure.LinkedGameObject.StatusSprite.sprite = SpriteStore.Instance.GetSpriteByName("axe");
                }
            }
            GameController.Instance.DeselectCell();
        };
    }


    public void DisableAndReset()
    {
        GameController.Instance.SelectionPreference = SelectionPreference.CreatureOnly;

        OrderTrayController.Instance.gameObject.SetActive(false);
        CellClickOrder = null;
    }

    private static void EnableAndClear()
    {
        OrderTrayController.Instance.gameObject.SetActive(true);
        foreach (Transform child in OrderTrayController.Instance.transform)
        {
            Destroy(child.gameObject);
        }
    }
    private void RemoveStructureClicked()
    {
        BuildButton.Text = "Remove Structure";
        GameController.Instance.SelectionPreference = SelectionPreference.CellOnly;
        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Data.Structure != null)
                {
                    var structure = cell.Data.Structure;

                    if (structure.IsBluePrint)
                    {
                        StructureController.Instance.DestroyStructure(structure);
                    }
                    else
                    {
                        Taskmaster.Instance.AddTask(new RemoveStructure(structure, cell.Data.Coordinates));
                        structure.LinkedGameObject.SpriteRenderer.color = Color.red;
                    }
                }
            }
        };
    }

    private void Start()
    {
        OrderTrayController.Instance.gameObject.SetActive(false);
        CreatureInfoPanel.Instance.gameObject.SetActive(false);
        CellInfoPanel.Instance.gameObject.SetActive(false);

        BuildButton = Instantiate(OrderButtonPrefab, transform);
        BuildButton.Button.onClick.AddListener(BuildTypeClicked);
        BuildButton.Text = "Select Building";
        BuildButton.Button.image.sprite = SpriteStore.Instance.GetSpriteByName("hammer");

        StockpileButton = Instantiate(OrderButtonPrefab, transform);
        StockpileButton.Button.onClick.AddListener(StockpileTypeClicked);
        StockpileButton.Text = "Place Stockpile";
        StockpileButton.Button.image.sprite = SpriteStore.Instance.GetSpriteByName("box");


        TaskButton = Instantiate(OrderButtonPrefab, transform);
        TaskButton.Button.onClick.AddListener(DesignateTypeClicked);
        TaskButton.Text = "Designate";
        TaskButton.Button.image.sprite = SpriteStore.Instance.GetSpriteByName("designate");
    }

    private void StockpileClicked(string itemTypeName)
    {
        StockpileButton.Text = $"Place {itemTypeName} Stockpile";

        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Data.Stockpile == null && cell.TravelCost > 0)
                {
                    var stockpile = StockpileController.Instance.AddStockpile(itemTypeName);
                    cell.AddContent(stockpile.gameObject);
                }
            }
        };
    }

    private void StockpileTypeClicked()
    {
        GameController.Instance.SelectionPreference = SelectionPreference.CellOnly;
        if (OrderTrayController.Instance.gameObject.activeInHierarchy)
        {
            DisableAndReset();
            StockpileButton.Text = "Place Stockpile";
        }
        else
        {
            EnableAndClear();

            foreach (var item in ItemController.Instance.AllItemTypes.Values)
            {
                var button = Instantiate(OrderButtonPrefab, OrderTrayController.Instance.transform);
                button.Button.onClick.AddListener(() => StockpileClicked(item.Data.ItemType));
                button.name = $"Place {item.Data.ItemType} Stockpile";
                button.Button.image.sprite = SpriteStore.Instance.GetSpriteByName(item.Data.SpriteName);
                button.Button.image.type = Image.Type.Tiled;
                button.Text = button.name;
            }
        }
    }
}