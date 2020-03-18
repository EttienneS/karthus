using Newtonsoft.Json;
using Structures;
using UnityEngine;

public class StructureRenderer : MonoBehaviour
{
    internal Structure Data;

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

}