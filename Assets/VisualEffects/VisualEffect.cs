using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;
using Random = UnityEngine.Random;

public class VisualEffect : MonoBehaviour
{
    internal float LifeSpan;

    public Vector3 Vector { get; set; }
    public bool Fade { get; set; }
    public SpriteRenderer Sprite { get; set; }
    public ParticleSystem ParticleSystem { get; set; }
    public Light2D Light { get; set; }

    public float PulseIntensity { get; set; } = -1;

    public void Awake()
    {
        Sprite = GetComponent<SpriteRenderer>();
        Light = GetComponent<Light2D>();
        ParticleSystem = GetComponent<ParticleSystem>();
        _startTime = Time.time;
    }

    private float _startTime;

    private void Update()
    {
        LifeSpan -= Time.deltaTime;
        if (LifeSpan <= 0)
        {
            Destroy(gameObject);
            return;
        }

        float t = (Time.time - _startTime) / LifeSpan;

        if (Sprite != null)
        {
            if (Fade)
            {
                Sprite.color = new Color(Sprite.color.r,
                                         Sprite.color.g,
                                         Sprite.color.b,
                                         Mathf.SmoothStep(0, 1, t));
            }

            transform.position += Vector;
        }
    }

    internal VisualEffect FadeUp()
    {
        Vector = new Vector2(Random.Range(-0.01f, 0.01f), Random.Range(0.01f, 0.05f));
        Fade = true;
        return this;
    }

    internal VisualEffect FadeDown()
    {
        Vector = new Vector2(Random.Range(-0.01f, 0.01f), Random.Range(-0.05f, -0.01f));
        Fade = true;
        return this;
    }

    internal void SetFade(float timeTillGone)
    {
        _startTime = Time.time;
        Fade = true;
        LifeSpan = timeTillGone;
    }

    internal VisualEffect Big()
    {
        transform.localScale = new Vector3(2, 2, 2);
        return this;
    }

    internal VisualEffect Regular()
    {
        transform.localScale = new Vector3(1, 1, 1);
        return this;
    }

    internal VisualEffect Small()
    {
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        return this;
    }

    internal VisualEffect Tiny()
    {
        transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        return this;
    }

    internal void Kill()
    {
        LifeSpan = 0;
    }
}