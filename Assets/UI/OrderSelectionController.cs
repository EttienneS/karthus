using System;
using UnityEngine;
using UnityEngine.UI;

public class OrderSelectionController : MonoBehaviour
{
    public OrderButton BuildButton;
    public OrderButton StockpileButton;

    public OrderButton OrderButtonPrefab;

    private static OrderSelectionController _instance;
    public delegate void CellClickedDelegate(Cell cell);

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

        CellClickOrder = cell =>
        {
            if (!cell.Filled)
            {
                var blueprint = StructureController.Instance.GetStructureBluePrint(structureName);
                cell.AddContent(blueprint.gameObject);
                cell.Filled = true;
                Taskmaster.Instance.AddTask(new Build(blueprint, cell));
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
            removeButton.Button.onClick.AddListener(() => RemoveStructureClicked());
            removeButton.name = "Remove Structure";
            removeButton.Text = removeButton.name;
            removeButton.Button.image.sprite = SpriteStore.Instance.GetSpriteByName("cancel");
        }
    }

    private void RemoveStructureClicked()
    {
        BuildButton.Text = "Remove Structure";

        CellClickOrder = cell =>
        {
            if (cell.Filled)
            {
                var structure = cell.GetComponentInChildren<Structure>();
                if (structure == null)
                {
                    return;
                }

                if (structure.Data.IsBluePrint)
                {
                    StructureController.Instance.RemoveStructure(structure);
                }
                else
                {
                    Taskmaster.Instance.AddTask(new RemoveStructure(structure, cell));
                    structure.SpriteRenderer.color = Color.red;
                }
            }
        };
    }

    private void DisableAndReset()
    {
        OrderTrayController.Instance.gameObject.SetActive(false);
        CellClickOrder = null;
    }

    private void Start()
    {
        OrderTrayController.Instance.gameObject.SetActive(false);

        BuildButton = Instantiate(OrderButtonPrefab, transform);
        BuildButton.Button.onClick.AddListener(BuildTypeClicked);
        BuildButton.Text = "Select Building";
        BuildButton.Button.image.sprite = SpriteStore.Instance.GetSpriteByName("hammer");


        StockpileButton = Instantiate(OrderButtonPrefab, transform);
        StockpileButton.Button.onClick.AddListener(StockpileTypeClicked);
        StockpileButton.Text = "Place Stockpile";
        StockpileButton.Button.image.sprite = SpriteStore.Instance.GetSpriteByName("box");

    }

    private void StockpileTypeClicked()
    {
        if (OrderTrayController.Instance.gameObject.activeInHierarchy)
        {
            DisableAndReset();
            StockpileButton.Text = "Place Stockpile";
        }
        else
        {
            EnableAndClear();

            foreach (var item in ItemController.Instance.AllItems.Values)
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

    private void StockpileClicked(string itemTypeName)
    {
        StockpileButton.Text = $"Place {itemTypeName} Stockpile";

        CellClickOrder = cell =>
        {
            if (!cell.Filled && cell.TravelCost > 0)
            {
                var stockpile = StockpileController.Instance.AddStockpile(itemTypeName);

                cell.AddContent(stockpile.gameObject);
                cell.Filled = true;
            }
        };
    }

    private static void EnableAndClear()
    {
        OrderTrayController.Instance.gameObject.SetActive(true);
        foreach (Transform child in OrderTrayController.Instance.transform)
        {
            Destroy(child.gameObject);
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}