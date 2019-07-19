using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Creature : MonoBehaviour
{
    internal SpriteRenderer SpriteRenderer;
    internal CreatureData Data = new CreatureData();
    internal SpriteRenderer Highlight;
    internal float RemainingTextDuration;
    internal TextMeshPro Text;
    internal LineRenderer LineRenderer;

    internal Light Light;

    internal float TempMaterialDuration;
    internal float TempMaterialDelta;

    public void Awake()
    {
        Highlight = transform.Find("Highlight").GetComponent<SpriteRenderer>();
        SpriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        Text = GetComponentInChildren<TextMeshPro>();

        LineRenderer = GetComponent<LineRenderer>();
        Light = GetComponent<Light>();

        Highlight.gameObject.SetActive(false);

    }

    public void ShowText(string text, float duration)
    {
        Text.text = text;
        Text.color = Color.white;

        RemainingTextDuration = duration + 1f;
    }

    public void ShowLine(Color color, params Vector3[] points)
    {
        LineRenderer.startColor = color;
        LineRenderer.endColor = color;

        LineRenderer.SetPositions(points);
        LineRenderer.positionCount = points.Length;

    }

    public void HideLine()
    {
        LineRenderer.positionCount = 0;
    }

    public void Update()
    {
        if (Game.TimeManager.Paused)
            return;

        Data.WorkTick += Time.deltaTime;

        if (Data.WorkTick >= Game.TimeManager.WorkInterval)
        {
            Data.WorkTick = 0;
            Work();
        }

        Data.InternalTick += Time.deltaTime;

        if (Data.InternalTick >= Game.TimeManager.TickInterval)
        {
            Data.InternalTick = 0;

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

            Data.Perceive();
            Data.Live();
        }

        UpdateMaterial();

    }

    internal void DisableHightlight()
    {
        Highlight.gameObject.SetActive(false);
    }

    internal void EnableHighlight(Color color)
    {
        Highlight.color = color;
        Highlight.gameObject.SetActive(true);
    }

    public void DrawAwareness()
    {
        var awareness = new List<Vector3> { Data.Coordinates.ToTopOfMapVector() };
        awareness.AddRange(Data.Awareness.Where(c => c.Neighbors.Any(n => !Data.Awareness.Contains(n)))
                                         .Select(c => c.Coordinates.ToTopOfMapVector()));
        awareness.Add(Data.Coordinates.ToTopOfMapVector());

        ShowLine(ColorConstants.BaseColor, awareness.ToArray());
    }

    internal void DoChannel(ManaColor color, float duration)
    {
        var col = color.GetActualColor();
        SpriteRenderer.material = Game.MaterialController.GetChannelingMaterial(col);
        TempMaterialDuration = duration * 2;
        TempMaterialDelta = 0;

        Light.color = col;
        Light.intensity = 0.4f;
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

    private void Work()
    {
        if (Data.Task == null)
        {
            var task = Data.Faction.GetTask(this);
            var context = $"{Data.GetGameId()} - {task} - {Game.TimeManager.Now}";

            Data.Know(context);
            task.Context = context;

            Data.Faction.AssignTask(Data, task);
            Data.Task = task;
        }
        else
        {
            try
            {
                Data.Faction.AssignTask(Data, Data.Task);

                if (Data.Task.Done())
                {
                    Data.Task.ShowDoneEmote();
                    Data.FreeResources(Data.Task.Context);
                    Data.Forget(Data.Task.Context);

                    Data.Faction.TaskComplete(Data.Task);
                    Data.Task = null;
                }
            }
            catch (TaskFailedException ex)
            {
                Debug.LogWarning($"Task failed: {ex}");
                Data.Faction.TaskFailed(Data.Task, ex.Message);
            }
        }
    }

    public Color CurrentColor { get; set; }

    public Sprite GetIcon()
    {
        return SpriteRenderer.sprite;
    }
}