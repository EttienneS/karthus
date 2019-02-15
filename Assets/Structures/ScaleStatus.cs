using UnityEngine;

public class ScaleStatus : MonoBehaviour
{
    private bool _scaled;
    private SpriteRenderer _spriteRenderer;
    private Color ColorLerp1 = new Color(1, 1, 1, 0.3f);
    private Color ColorLerp2 = new Color(1f, 0f, 0f, 0.6f);

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_spriteRenderer.sprite != null)
        {
            if (!_scaled)
            {
                var bounds = _spriteRenderer.bounds;
                var factor = 0.5f / bounds.size.y;
                transform.localScale = new Vector3(factor, factor, factor);
                _scaled = true;
            }
            else
            {
                _spriteRenderer.color = Color.Lerp(ColorLerp1, ColorLerp2, Mathf.PingPong(Time.time, 1));
            }
        }
    }
}