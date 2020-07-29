using UnityEngine;
using UnityEngine.UI;

public class StoragePanel : MonoBehaviour
{
    public Text ZoneInfoText;
    internal StorageZone Zone;

    public void AddAllowed()
    {
        Game.Instance.UIController.ShowFilterView("Choose items to allow:",
                                                  Game.Instance.ItemController.GetAllItemOptions(),
                                                  (option) => Zone.Filter.AddAllowedItem(option.Name));
    }

    public void AddBlocked()
    {
        Game.Instance.UIController.ShowFilterView("Choose items to allow:",
                                                  Game.Instance.ItemController.GetAllItemOptions(),
                                                  (option) => Zone.Filter.AddBlockedItem(option.Name));
    }

    public void ClearFilter()
    {
        Zone.Filter.Clear();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show(StorageZone zone)
    {
        gameObject.SetActive(true);

        Zone = zone;
    }

    public void Update()
    {
        ZoneInfoText.text = $"Capacity: {Zone.GetFreeCellCount()}/{Zone.GetMaxItemCapacity()}\n";
        ZoneInfoText.text = $"Filter: {Zone.Filter}\n";
    }
}