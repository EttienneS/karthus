using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class OrderSelectionController : MonoBehaviour
{
    public OrderButton OrderButtonPrefab;
    private static OrderSelectionController _instance;

    public delegate void CellClickedDelegate(List<CellData> cell);

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

    private OrderButton CreateOrderButton(string text, UnityAction action, string sprite, bool isSubButton = true)
    {
        // create a top level button for an order type
        var button = Instantiate(OrderButtonPrefab, isSubButton ? OrderTrayController.Instance.transform : transform);
        button.Button.onClick.AddListener(action);
        button.Text = text;
        button.Button.image.sprite = SpriteStore.Instance.GetSpriteByName(sprite);

        return button;
    }


    private void Start()
    {
        OrderTrayController.Instance.gameObject.SetActive(false);
        CreatureInfoPanel.Instance.gameObject.SetActive(false);
        CellInfoPanel.Instance.gameObject.SetActive(false);

        BuildButton = CreateOrderButton(DefaultBuildText, BuildTypeClicked, "hammer", false);
        StockpileButton = CreateOrderButton(DefaultStockpileText, StockpileTypeClicked, "box", false);
        TaskButton = CreateOrderButton(DefaultDesignateText, DesignateTypeClicked, "designate", false);
    }
}