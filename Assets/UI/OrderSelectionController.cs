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
        Game.Controller.SelectionStartWorld = Vector3.zero;
        Game.Controller.CurrentDragMode = DragMode.SelectionRectangle;
        Game.Controller.MouseSpriteRenderer.size = new Vector2(1, 1);

        Game.Controller.SelectionPreference = SelectionPreference.Entity;

        Game.OrderTrayController.gameObject.SetActive(false);
        CellClickOrder = null;

        Game.OrderInfoPanel.Hide();

    }

    private static void EnableAndClear()
    {
        Game.OrderTrayController.gameObject.SetActive(true);
        foreach (Transform child in Game.OrderTrayController.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private OrderButton CreateOrderButton(string text, UnityAction action, string sprite, bool isSubButton = true)
    {
        return CreateOrderButton(text, action, Game.SpriteStore.GetSprite(sprite), isSubButton);
    }

    private OrderButton CreateOrderButton(string text, UnityAction action, Sprite sprite, bool isSubButton = true)
    {
        // create a top level button for an order type
        var button = Instantiate(OrderButtonPrefab, isSubButton ? Game.OrderTrayController.transform : transform);
        button.Button.onClick.AddListener(action);
        button.Text = text;
        button.Button.image.sprite = sprite;

        return button;
    }

    private void Start()
    {
        Game.OrderTrayController.gameObject.SetActive(false);
        Game.EntityInfoPanel.gameObject.SetActive(false);
        Game.OrderInfoPanel.Hide();

        BuildButton = CreateOrderButton(DefaultBuildText, BuildTypeClicked, "hammer", false);
        ZonesButton = CreateOrderButton(DefaultZoneText, ZoneTypeClicked, "plus_t", false);
        TaskButton = CreateOrderButton(DefaultDesignateText, DesignateTypeClicked, "designate", false);
        ConstructButton = CreateOrderButton(DefaultConstructText, ConstructTypeClicked, "construct", false);
    }
}