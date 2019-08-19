using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreatureRenderer : MonoBehaviour
{
    internal CreatureData Data = new CreatureData();
    internal SpriteRenderer Highlight;
    internal Light Light;
    internal LineRenderer LineRenderer;
    internal float RemainingTextDuration;
    internal SpriteRenderer SpriteRenderer;
    internal float TempMaterialDelta;
    internal float TempMaterialDuration;
    internal TextMeshPro Text;

    public void Awake()
    {
        Highlight = transform.Find("Highlight").GetComponent<SpriteRenderer>();
        SpriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        Text = GetComponentInChildren<TextMeshPro>();

        LineRenderer = GetComponent<LineRenderer>();
        Light = GetComponent<Light>();

        Highlight.gameObject.SetActive(false);
    }

    public void DrawAwareness()
    {
        var awareness = new List<Vector3> { Data.Coordinates.ToTopOfMapVector() };
        awareness.AddRange(Data.Awareness.Where(c => c.Neighbors.Any(n => !Data.Awareness.Contains(n)))
                                         .Select(c => c.Coordinates.ToTopOfMapVector()));
        awareness.Add(Data.Coordinates.ToTopOfMapVector());

        ShowLine(ColorConstants.BaseColor, awareness.ToArray());
    }

    public Sprite GetIcon()
    {
        return SpriteRenderer.sprite;

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

        RemainingTextDuration = duration + 1f;
    }

    public void Update()
    {
        if (Data.Update(Time.deltaTime))
        {
            if (Random.value > 0.65)
            {
                Data.Task?.ShowBusyEmote();
            }

            UpdateFloatingText();

            if (Highlight.gameObject.activeInHierarchy)
            {
                DrawAwareness();
            }
            else
            {
                HideLine();
            }

            UpdateMaterial();
        }
    }

    internal void DisableHightlight()
    {
        Highlight.gameObject.SetActive(false);
    }

    internal void DisplayChannel(ManaColor color, float duration)
    {
        var col = color.GetActualColor();
        SpriteRenderer.material = Game.MaterialController.GetChannelingMaterial(col);
        TempMaterialDuration = duration * 2;
        TempMaterialDelta = 0;

        Light.color = col;
        Light.intensity = 0.4f;
    }

    internal void EnableHighlight(Color color)
    {
        Highlight.color = color;
        Highlight.gameObject.SetActive(true);
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

    private void UpdateMaterial()
    {
        //if (SpriteRenderer.material.name != Game.MaterialController.ChannelingMaterial.name)
        //{
        //    SpriteRenderer.SetBoundMaterial(Data.CurrentCell.Bound);
        //}

        TempMaterialDelta += Time.deltaTime;
        if (TempMaterialDelta >= TempMaterialDuration)
        {
            Light.color = Color.white;
            Light.intensity = 0.1f;

            SpriteRenderer.SetBoundMaterial(Data.CurrentCell.Bound);
        }
    }
}