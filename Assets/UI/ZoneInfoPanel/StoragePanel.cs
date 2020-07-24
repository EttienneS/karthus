using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Structures;
public class StoragePanel : MonoBehaviour
{
    internal StorageZone Zone;

    public Text ZoneInfoText;

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
        ZoneInfoText.text = $"Capacity: {Zone.Fill}/{Zone.Capacity}\n"; ;
    }
}