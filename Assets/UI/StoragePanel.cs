using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Structures;
public class StoragePanel : MonoBehaviour
{
    internal StorageZone Zone;

    public Text ZoneInfoText;

    public InputField FilterInput;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show(StorageZone zone)
    {
        gameObject.SetActive(true);

        Zone = zone;

        FilterInput.text = zone.Filter;
    }

    public void Update()
    {
        if (Zone.Containers.Count() > 0)
        {
            var text = $"Capacity: {Zone.Fill}/{Zone.Capacity}\n";
            text += $"Containers: {Zone.Containers.Count()}/{Zone.Cells.Count}\n\n";

            foreach (var container in Zone.Containers)
            {
                var item = container.GetContainedItemTemplate();

                if (item != null)
                {
                    text += $"\t{item.Name} - {container.Count}\n";
                }
            }
            ZoneInfoText.text = text;
        }
        else
        {
            ZoneInfoText.text = "No containers in zone.  Build storage containers to use storage zone.";
        }
    }

    public void FilterChanged()
    {
        Game.Instance.Typing = true;
        Game.ZoneController.Refresh(Zone);
    }

    public void DoneEditing()
    {
        Zone.SetFilter(FilterInput.text);
        Game.Instance.Typing = false;
    }
}