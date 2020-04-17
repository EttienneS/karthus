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

    private OrderButton CreateOrderButton(UnityAction action, UnityAction onHover, string sprite, bool isSubButton = true)
    {
        return CreateOrderButton(action, onHover, Game.Instance.SpriteStore.GetSprite(sprite), isSubButton);
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

        BuildButton = CreateOrderButton(BuildTypeClicked, null, "hammer", false);
        ZonesButton = CreateOrderButton(ZoneTypeClicked, null, "plus_t", false);
        TaskButton = CreateOrderButton(DesignateTypeClicked, null, "designate", false);
        ConstructButton = CreateOrderButton(ConstructTypeClicked, null, "construct", false);
    }
}