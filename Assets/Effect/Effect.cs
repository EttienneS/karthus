using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Effect : MonoBehaviour
{
    internal float LifeSpan;

    public Vector3 Vector { get; set; }
    public bool Fade { get; set; }
    public SpriteRenderer Sprite { get; set; }
    public ParticleSystem ParticleSystem { get; set; }

    public float PulseIntensity { get; set; } = -1;

    public void Awake()
    {
        Sprite = GetComponent<SpriteRenderer>();
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

        if (PulseIntensity > 0)
        {
            var pulse = Mathf.SmoothStep(0, 1, t);
            transform.localScale += new Vector3(pulse, pulse);
        }
    }

    internal Effect Pulsing(float intensity)
    {
        PulseIntensity = intensity;
        return this;
    }

    internal Effect FadeUp()
    {
        Vector = new Vector2(Random.Range(-0.01f, 0.01f), Random.Range(0.01f, 0.05f));
        Fade = true;
        return this;
    }

    internal Effect FadeDown()
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

    internal Effect Big()
    {
        transform.localScale = new Vector3(2, 2, 2);
        return this;
    }

    internal Effect Regular()
    {
        transform.localScale = new Vector3(1, 1, 1);
        return this;
    }

    internal Effect Small()
    {
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        return this;
    }

    internal Effect Tiny()
    {
        transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        return this;
    }

    internal void Kill()
    {
        LifeSpan = 0;
    }
}