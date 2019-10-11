using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;
using Random = UnityEngine.Random;

public class VisualEffect : MonoBehaviour
{
    public bool FadeOut;

    internal float LifeSpan;
    internal float FullSpan = -1;

    private float _startTime;
    public bool Fade { get; set; }
    public Light2D Light { get; set; }
    public ParticleSystem ParticleSystem { get; set; }
    public float Intensity { get; set; } = -1;
    public float StartIntensity { get; set; } = -1;

    public SpriteRenderer Sprite { get; set; }
    public Vector3 Vector { get; set; }

    public void Awake()
    {
        Sprite = GetComponent<SpriteRenderer>();
        Light = GetComponent<Light2D>();
        ParticleSystem = GetComponent<ParticleSystem>();
        _startTime = Time.time;
    }

    internal VisualEffect Big()
    {
        transform.localScale = new Vector3(2, 2, 2);
        return this;
    }

    internal VisualEffect FadeDown()
    {
        Vector = new Vector2(Random.Range(-0.001f, 0.001f), Random.Range(-0.005f, -0.001f));
        Fades();
        return this;
    }

    internal VisualEffect FadeTo(Vector2 vector)
    {
        Vector = vector;
        Fades();

        return this;
    }

    internal VisualEffect FadeUp()
    {
        Vector = new Vector2(Random.Range(-0.001f, 0.001f), Random.Range(0.001f, 0.005f));
        Fades();

        return this;
    }

    internal void Kill()
    {
        LifeSpan = 0;
    }

    internal VisualEffect Regular()
    {
        transform.localScale = new Vector3(1, 1, 1);
        return this;
    }

    internal VisualEffect Fades(bool fadeOut = false)
    {
        _startTime = Time.time;
        Fade = true;
        FadeOut = fadeOut;
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

    private void Update()
    {
        if (Game.TimeManager.Paused)
            return;

        if (FullSpan < 0)
        {
            FullSpan = LifeSpan;
        }

        if (StartIntensity < 0)
        {
            StartIntensity = Intensity;
        }

        LifeSpan -= Time.deltaTime;
        if (LifeSpan <= 0)
        {
            Destroy(gameObject);
            return;
        }

        float t = (Time.time - _startTime) / LifeSpan;

        var step = FadeOut ? Mathf.SmoothStep(0, FullSpan, t) : Mathf.SmoothStep(FullSpan, 0, t);

        if (Sprite != null)
        {
            if (Fade)
            {
                Sprite.color = new Color(Sprite.color.r,
                                         Sprite.color.g,
                                         Sprite.color.b,
                                         step);
            }
        }

        if (Light != null)
        {
            if (Fade)
            {
                Light.intensity = Intensity * (LifeSpan / FullSpan);
            }
        }

        transform.position += Vector;
    }
}