using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoxButtonImageSwap : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    private Sprite _selectedSprite;
    private Sprite _unselectedSprite;
    private Button _button;
    public void OnPointerEnter(PointerEventData eventData)
    {
        _button.image.sprite = _selectedSprite; 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _button.image.sprite = _unselectedSprite;
    }

    private void Start()
    {
        _button = GetComponent<Button>();

        _unselectedSprite = Resources.Load<Sprite>("uibox");
        _selectedSprite = Resources.Load<Sprite>("uiboxselected");
    }
}