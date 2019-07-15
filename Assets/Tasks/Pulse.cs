using Newtonsoft.Json;
using UnityEngine;

public class Pulse : TaskBase
{
    public float[] ColorArray1;
    public float[] ColorArray2;
    public string GameObjectId;
    public float TimeLeft;
    public float Frequency;

    [JsonIgnore]
    private SpriteRenderer _attachedRenderer;

    public Pulse()
    {
    }

    public Pulse(string id, Color color1, Color color2, float duration, float frequency)
    {
        GameObjectId = id;
        ColorArray1 = color1.ToFloatArray();
        ColorArray2 = color2.ToFloatArray();
        TimeLeft = duration;
        Frequency = frequency;
    }

    public override bool Done()
    {
        if (_attachedRenderer == null)
        {
            _attachedRenderer = IdService.GetSpriteRendererForId(GameObjectId);
        }

        if (TimeLeft != float.MaxValue)
        {
            TimeLeft -= Time.deltaTime;
        }

        _attachedRenderer.color = Color.Lerp(ColorArray1.ToColor(), ColorArray2.ToColor(), Mathf.PingPong(Time.time, Frequency));

        if (TimeLeft <= 0)
        {
            return true;
        }

        return false;
    }

}