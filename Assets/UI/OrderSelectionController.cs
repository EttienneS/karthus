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
        Game.Controller.SelectionPreference = SelectionPreference.CreatureOrStructure;

        Game.OrderTrayController.gameObject.SetActive(false);
        CellClickOrder = null;
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
        return CreateOrderButton(text, action, Game.SpriteStore.GetSpriteByName(sprite), isSubButton);
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
        Game.CreatureInfoPanel.gameObject.SetActive(false);

        BuildButton = CreateOrderButton(DefaultBuildText, BuildTypeClicked, "hammer", false);
        TaskButton = CreateOrderButton(DefaultDesignateText, DesignateTypeClicked, "designate", false);
        ConstructButton = CreateOrderButton(DefaultConstructText, ConstructTypeClicked, "construct", false);
    }
}