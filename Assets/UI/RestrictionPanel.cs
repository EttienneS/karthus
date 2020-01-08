using UnityEngine;
using UnityEngine.UI;

public class RestrictionPanel : MonoBehaviour
{
    internal RestrictionZone Zone;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show(RestrictionZone zone)
    {
        gameObject.SetActive(true);

        Zone = zone;
    }
}
