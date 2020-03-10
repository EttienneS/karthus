using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CreatureRenderer : MonoBehaviour
{
    public UnityEngine.Experimental.Rendering.Universal.Light2D Light;
    internal Creature Data = new Creature();
    internal SpriteRenderer Highlight;
    internal LineRenderer LineRenderer;
    internal SpriteRenderer MainRenderer;
    internal float RemainingTextDuration;
    internal TextMeshPro Text;

    public void Awake()
    {
        Highlight = transform.Find("Highlight").GetComponent<SpriteRenderer>();
        var mainSprite = transform.Find("Sprite");
        MainRenderer = mainSprite.GetComponent<SpriteRenderer>();

        Text = GetComponentInChildren<TextMeshPro>();

        LineRenderer = GetComponent<LineRenderer>();

        Highlight.gameObject.SetActive(false);
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

    public void Update()
    {
        UpdateFloatingText();

        if (Data.Update(Time.deltaTime))
        {
            if (Game.TimeManager.Data.Hour < 6 || Game.TimeManager.Data.Hour > 18)
            {
                EnableLight();
            }
            else
            {
                DisableLight();
            }
        }
    }

    internal void DisableHightlight()
    {
        if (Highlight != null && Highlight.gameObject != null)
        {
            Highlight.gameObject.SetActive(false);
        }
    }


    internal void EnableHighlight(Color color)
    {
        if (Highlight != null)
        {
            Highlight.color = color;
            Highlight.gameObject.SetActive(true);
        }
    }

    internal void EnableLight()
    {
        if (Light != null)
        {
            Light.gameObject.SetActive(true);
        }
    }

    void OnDrawGizmos()
    {
        if (Data.UnableToFindPath)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Data.Cell.Vector + new Vector3(0, 0, 5), Game.Map.GetCellAtCoordinate(Data.TargetCoordinate).Vector + new Vector3(0, 0, 5));
        }
    }

    internal void DisableLight()
    {
        if (Light != null)
        {
            Light.gameObject.SetActive(false);
        }
    }

    internal void UpdatePosition()
    {
        transform.position = new Vector2(Data.X, Data.Y);

        Data.UpdateSprite();
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

}