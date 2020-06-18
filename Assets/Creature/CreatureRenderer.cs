using System;
using TMPro;
using UnityEngine;

public enum AnimationType
{
    Idle, Running, Jumping
}

public class CreatureRenderer : MonoBehaviour
{
    public SpriteRenderer Highlight;
    public Light Light;
    public TextMeshPro Text;
    internal Creature Data = new Creature();
    internal float RemainingTextDuration;
    public Animator Animator;

    public void Awake()
    {
        Highlight.gameObject.SetActive(false);
    }

    public void ShowText(string text, float duration)
    {
        Text.text = text;
        Text.color = ColorConstants.WhiteBase;

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
            if (Game.Instance.TimeManager.Data.Hour < 6 || Game.Instance.TimeManager.Data.Hour > 18)
            {
                EnableLight();
            }
            else
            {
                DisableLight();
            }
        }

        if (Animator != null)
        {
            foreach (AnimationType animationState in Enum.GetValues(typeof(AnimationType)))
            {
                Animator.SetBool(animationState.ToString(), animationState == Data.Animation);
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

    internal void DisableLight()
    {
        if (Light != null)
        {
            Light.gameObject.SetActive(false);
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

    internal void UpdatePosition()
    {
        transform.position = new Vector3(Data.X, Data.Cell.Y, Data.Z);
        transform.eulerAngles = new Vector3(0, (int)Data.Facing * 45f, 0);
    }

    private void OnDrawGizmos()
    {
        if (Data.UnableToFindPath)
        {
            Gizmos.color = ColorConstants.RedBase;
        }
        else
        {
            Gizmos.color = ColorConstants.GreenBase;
        }

        if (Data.Path != null)
        {
            Cell lastNode = null;
            foreach (var cell in Data.Path)
            {
                if (lastNode != null)
                {
                    Gizmos.DrawLine(lastNode.Vector + new Vector3(0, 0, 5), cell.Vector + new Vector3(0, 0, 5));
                }
                lastNode = cell;
            }
        }

        Gizmos.DrawCube(Game.Instance.Map.GetCellAtCoordinate(Data.TargetCoordinate).Vector + new Vector3(0, 0, 5), new Vector3(0.1f, 0.1f, 0.1f));
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