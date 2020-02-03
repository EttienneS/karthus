using UnityEngine;

public class AreaPanel : MonoBehaviour
{
    internal AreaZone Zone;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show(AreaZone zone)
    {
        gameObject.SetActive(true);

        Zone = zone;
    }
}