using UnityEngine;
using UnityEngine.UI;

public class StoragePanel : MonoBehaviour
{
    internal StorageZone Zone;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show(StorageZone zone)
    {
        gameObject.SetActive(true);

        Zone = zone;
    }
}
