using UnityEngine.UI;

public partial class OrderSelectionController //.Stockpile
{
    public const string DefaultStockpileText = "Place Stockpile";
    internal OrderButton StockpileButton;

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
            StockpileButton.Text = DefaultStockpileText;
        }
        else
        {
            EnableAndClear();

            foreach (var item in ItemController.Instance.AllItemTypes.Values)
            {
                var button = CreateOrderButton(CutText, () => StockpileClicked(item.Data.ItemType), item.Data.SpriteName);
                button.Button.image.type = Image.Type.Tiled;
                button.name = $"Place {item.Data.ItemType} Stockpile";
                button.Text = button.name;
            }
        }
    }
}