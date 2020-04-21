using Newtonsoft.Json;
using Structures;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class StructureRenderer : MonoBehaviour
{
    public CompositeShadowCaster2D CompositeCaster;
    public ShadowCaster2D ShadowCaster;
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

    public void DisableShadow()
    {
        if (Game.Instance.EnableShadows)
        {
            ShadowCaster.enabled = false;
            CompositeCaster.enabled = false;
        }
    }

    public void EnableShadow()
    {
        if (Game.Instance.EnableShadows)
        {
            ShadowCaster.enabled = true;
            CompositeCaster.enabled = true;
        }
    }

    public void Start()
    {
        if (Data.IsShadowCaster() && !Data.IsBluePrint)
        {
            EnableShadow();
        }
        else
        {
            DisableShadow();
        }
    }

    internal void UpdatePosition()
    {
        transform.position = Data.Vector;
    }
}