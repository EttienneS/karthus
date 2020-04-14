using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class ItemRenderer : MonoBehaviour
{
    internal Item Data = new Item();
    internal TextMeshPro Text;

    public void Start()
    {
        Text = GetComponentInChildren<TextMeshPro>();
    }

    [JsonIgnore]
    private SpriteRenderer _spriteRenderer;

    [JsonIgnore]
    public SpriteRenderer SpriteRenderer
    {
        get
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            }

            return _spriteRenderer;
        }
    }

    internal void UpdatePosition()
    {
        transform.position = Data.Vector;
    }

    public void Update()
    {
        if (Data.Amount <= 0)
        {
            Game.Instance.ItemController.DestroyItem(Data);
        }
        else
        {
            Text.text = Data.Amount.ToString();
        }
    }
}