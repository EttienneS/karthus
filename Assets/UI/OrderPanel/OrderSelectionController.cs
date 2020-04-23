using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class OrderSelectionController : MonoBehaviour
{
    public OrderButton OrderButtonPrefab;

    public delegate void CellClickedDelegate(List<Cell> cell);

    public CellClickedDelegate CellClickOrder { get; set; }

    public void DisableAndReset()
    {
        Game.Instance.SelectionStartWorld = Vector3.zero;
        Game.Instance.MouseSpriteRenderer.size = new Vector2(1, 1);

        Game.Instance.SelectionPreference = SelectionPreference.Anything;

        Game.Instance.OrderTrayController.gameObject.SetActive(false);
        CellClickOrder = null;

        Game.Instance.OrderInfoPanel.Hide();
    }

    private static void EnableAndClear()
    {
        Game.Instance.OrderTrayController.gameObject.SetActive(true);
        foreach (Transform child in Game.Instance.OrderTrayController.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private OrderButton CreateOrderButton(UnityAction action, UnityAction onHover, string spriteName, string colorHex = "#ffffff", bool isSubButton = true)
    {
        var btn = CreateOrderButton(action, onHover, Game.Instance.SpriteStore.GetSprite(spriteName), isSubButton);
        btn.Button.image.color = colorHex.GetColorFromHex();
        return btn;
    }

    private OrderButton CreateOrderButton(UnityAction action, UnityAction onHover, Sprite sprite, bool isSubButton = true)
    {
        // create a top level button for an order type
        var button = Instantiate(OrderButtonPrefab, isSubButton ? Game.Instance.OrderTrayController.transform : transform);
        button.Button.onClick.AddListener(action);
        button.OnMouseEnter = onHover;
        button.Button.image.sprite = sprite;

        return button;
    }

    private void Start()
    {
        Game.Instance.OrderTrayController.gameObject.SetActive(false);
        Game.Instance.OrderInfoPanel.Hide();

        BuildButton = CreateOrderButton(BuildTypeClicked, null, "hammer", "#ffffff", false);
        ZonesButton = CreateOrderButton(ZoneTypeClicked, null, "plus_t", "#ffffff", false);
        TaskButton = CreateOrderButton(DesignateTypeClicked, null, "designate", "#ffffff", false);
        ConstructButton = CreateOrderButton(ConstructTypeClicked, null, "construct", "#ffffff", false);
    }
}