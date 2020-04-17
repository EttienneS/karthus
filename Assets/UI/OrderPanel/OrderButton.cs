using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OrderButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button Button;

    public UnityAction OnMouseEnter;
    public UnityAction OnMouseExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouseEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnMouseExit?.Invoke();
    }

    // Start is called before the first frame update
    private void Awake()
    {
        Button = GetComponentInChildren<Button>();
    }
}