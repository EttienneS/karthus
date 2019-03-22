﻿public partial class OrderSelectionController //.Stockpile
{
    public const string DefaultStockpileText = "Place Stockpile";
    internal OrderButton StockpileButton;

    private void StockpileClicked(string itemCategory)
    {
        StockpileButton.Text = $"Place {itemCategory} Stockpile";

        CellClickOrder = cells =>
        {
            foreach (var cell in cells)
            {
                if (cell.Stockpile == null && cell.TravelCost > 0)
                {
                    var stockpile = StockpileController.Instance.AddStockpile(itemCategory);
                    cell.AddContent(stockpile.gameObject);
                }
            }
        };
    }

    private void StockpileTypeClicked()
    {
        GameController.Instance.SelectionPreference = SelectionPreference.Cell;
        if (OrderTrayController.Instance.gameObject.activeInHierarchy)
        {
            DisableAndReset();
            StockpileButton.Text = DefaultStockpileText;
        }
        else
        {
            EnableAndClear();

            foreach (var category in ItemController.Instance.ItemCategoryIndex.Keys)
            {
                var button = CreateOrderButton(CutText, () => StockpileClicked(category), category);
                button.name = $"Place {category} Stockpile";
                button.Text = button.name;
            }
        }
    }
}