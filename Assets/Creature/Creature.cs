using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Creature : MonoBehaviour
{
    internal SpriteRenderer Sprite;
    internal CreatureData Data = new CreatureData();
    internal SpriteRenderer Highlight;
    internal float RemainingTextDuration;
    internal TextMeshPro Text;

    internal Light Light;

    internal float TempMaterialDuration;
    internal float TempMaterialDelta;

    public void Awake()
    {
        Text = transform.Find("Text").GetComponent<TextMeshPro>();
        Highlight = transform.Find("Highlight").GetComponent<SpriteRenderer>();
        Light = GetComponentInChildren<Light>();
        Highlight.gameObject.SetActive(false);
        Sprite = transform.Find("Sprite").gameObject.GetComponent<SpriteRenderer>();
    }

    public void ShowText(string text, float duration)
    {
        Text.text = text;
        Text.color = Color.white;

        RemainingTextDuration = duration + 1f;
    }

    public void Update()
    {
        if (Game.TimeManager.Paused) return;

        Work();

        UpdateSelf();
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

    internal void SetTempMaterial(Material material, float duration)
    {
        Sprite.material = material;
        TempMaterialDuration = duration * 2;
        TempMaterialDelta = 0;

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

    private void UpdateSelf()
    {
        Data.InternalTick += Time.deltaTime;

        var thoughts = new List<string>();
        if (Data.InternalTick >= Game.TimeManager.TickInterval)
        {
            Data.InternalTick = 0;

            if (Random.value > 0.65)
            {
                Data.Task?.ShowBusyEmote();
            }

            Data.ValueProperties[Prop.Hunger] += Random.value;
            Data.ValueProperties[Prop.Energy] -= Random.Range(0.1f, 0.25f);
        }

        if (thoughts.Count > 0 && Random.value > 0.9)
        {
            ShowText(thoughts[Random.Range(0, thoughts.Count - 1)], 2f);
        }

        UpdateFloatingText();
        UpdateMaterial();
    }

    private void UpdateMaterial()
    {
        TempMaterialDelta += Time.deltaTime;
        if (TempMaterialDelta >= TempMaterialDuration)
        {
            Sprite.material = Game.MaterialController.DefaultMaterial;
            Light.color = Color.white;
            Light.intensity = 0.1f;
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

                if (!Data.Task.Done())
                {
                    Data.Task.Update();
                }
                else
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


    public string SpriteName;

    
    public Color CurrentColor { get; set; }

    public Sprite GetIcon()
    {
        return Sprite.sprite;
    }

}