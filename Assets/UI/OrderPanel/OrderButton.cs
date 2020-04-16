using UnityEngine;
using UnityEngine.UI;

public class OrderButton : MonoBehaviour
{
    
    public Button Button;

    // Start is called before the first frame update
    private void Awake()
    {
        Button = GetComponentInChildren<Button>();
    }
}