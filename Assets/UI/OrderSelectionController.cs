using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class OrderSelectionController : MonoBehaviour
{
    public OrderButton OrderButtonPrefab;
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

    private void CreateOrderTypeButton(string text, UnityAction action, string sprite)
    {
        // create a top level button for an order type
        BuildButton = Instantiate(OrderButtonPrefab, transform);
        BuildButton.Button.onClick.AddListener(action);
        BuildButton.Text = text;
        BuildButton.Button.image.sprite = SpriteStore.Instance.GetSpriteByName(sprite);
    }

    private void Start()
    {
        OrderTrayController.Instance.gameObject.SetActive(false);
        CreatureInfoPanel.Instance.gameObject.SetActive(false);
        CellInfoPanel.Instance.gameObject.SetActive(false);

        CreateOrderTypeButton(DefaultBuildText, BuildTypeClicked, "hammer");
        CreateOrderTypeButton(DefaultStockpileText, StockpileTypeClicked, "box");
        CreateOrderTypeButton(DefaultDesignateText, DesignateTypeClicked, "designate");
    }
}