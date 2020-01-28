using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class CreatureRenderer : MonoBehaviour
{
    internal Creature Data = new Creature();
    internal SpriteRenderer Highlight;
    internal LineRenderer LineRenderer;
    internal float RemainingTextDuration;
    internal SpriteRenderer MainRenderer;
    internal TextMeshPro Text;

    public void Awake()
    {
        Highlight = transform.Find("Highlight").GetComponent<SpriteRenderer>();
        var mainSprite = transform.Find("Sprite");
        MainRenderer = mainSprite.GetComponent<SpriteRenderer>();

        Text = GetComponentInChildren<TextMeshPro>();

        LineRenderer = GetComponent<LineRenderer>();

        Highlight.gameObject.SetActive(false);
        Light.gameObject.SetActive(false);
    }

    public void DrawAwareness()
    {
        var awareness = new List<Vector3> { Data.Vector };
        awareness.AddRange(Data.Awareness.Where(c => c.Neighbors.Any(n => !Data.Awareness.Contains(n)))
                                         .Select(c => c.Vector));
        awareness.Add(Data.Cell.Vector);

        ShowLine(Color.white, awareness.ToArray());
    }

    public Sprite GetIcon()
    {
        return MainRenderer.sprite;
    }

    public void HideLine()
    {
        LineRenderer.positionCount = 0;
    }

    public void ShowLine(Color color, params Vector3[] points)
    {
        LineRenderer.startColor = color;
        LineRenderer.endColor = color;

        LineRenderer.SetPositions(points);
        LineRenderer.positionCount = points.Length;
    }

    public void ShowText(string text, float duration)
    {
        Text.text = text;
        Text.color = Color.white;

        RemainingTextDuration = duration;
    }

    public void Start()
    {
        Data.Start();
    }

    internal void UpdatePosition()
    {
        transform.position = new Vector2(Data.X, Data.Y);

        Data.UpdateSprite();
    }


    public void Update()
    {
        UpdateFloatingText();

        if (Data.Update(Time.deltaTime))
        {
        }
        UpdateLight();
    }

    internal void DisableHightlight()
    {
        if (Highlight != null && Highlight.gameObject != null)
        {
            Highlight.gameObject.SetActive(false);
        }
    }
     
    internal void DisplayChannel(ManaColor color, float duration)
    {
        var col = color.GetActualColor();
        MainRenderer.material = Game.MaterialController.GetChannelingMaterial(col);
    }

    internal void EnableLight()
    {
        if (Light != null)
        {
            Light.gameObject.SetActive(true);
        }
    }

    public UnityEngine.Experimental.Rendering.Universal.Light2D Light;

    internal void EnableHighlight(Color color)
    {
        if (Highlight != null)
        {
            Highlight.color = color;
            Highlight.gameObject.SetActive(true);
        }
    }

    private void UpdateFloatingText()
    {
        if (RemainingTextDuration > 0)
        {
            RemainingTextDuration -= Time.deltaTime;

            if (RemainingTextDuration < 1f)
            {
                Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, RemainingTextDuration);
            }
        }
        else
        {
            Text.text = "";
        }
    }

    private void UpdateLight()
    {
        if (Data.ManaPool != null)
        {
            var totalMana = Data.ManaPool.Sum(t => t.Value.Total);
            Light.pointLightOuterRadius = 1 + Mathf.PingPong(Time.time, 0.1f);
            Light.intensity = (totalMana / 15.0f) * (1 + Mathf.PingPong(Time.time, 0.1f));
            Light.pointLightInnerAngle = Mathf.Min(totalMana * 5.0f, 360.0f);

            Light.color = Data.ManaPool.GetManaWithMost().GetActualColor();
        }
    }
}