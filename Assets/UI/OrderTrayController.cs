using UnityEngine;

public class OrderTrayController : MonoBehaviour
{
    private static OrderTrayController _instance;

    public static OrderTrayController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("OrderTray").GetComponent<OrderTrayController>();
            }

            return _instance;
        }
    }
}