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
        if (LifeSpan < 0)
        {
            Destroy(gameObject);
        }

        if (Sprite != null)
        {
            if (Fade)
            {
                float t = (Time.time - _startTime) / LifeSpan;
                Sprite.color = new Color(Sprite.color.r,
                                         Sprite.color.g,
                                         Sprite.color.b,
                                         Mathf.SmoothStep(0, 1, t));
            }

            transform.position += Vector;
        }
    }

    internal void FadeUp()
    {
        Vector = new Vector2(Random.Range(-0.01f, 0.01f), Random.Range(0.01f, 0.05f));
        Fade = true;
    }

    internal void FadeDown()
    {
        Vector = new Vector2(Random.Range(-0.01f, 0.01f), Random.Range(-0.05f, -0.01f));
        Fade = true;
    }
}