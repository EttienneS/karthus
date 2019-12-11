using Newtonsoft.Json;
using UnityEngine;

public class ItemRenderer : MonoBehaviour
{
    internal Item Data = new Item();

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

        //Data.UpdateSprite();
    }
}