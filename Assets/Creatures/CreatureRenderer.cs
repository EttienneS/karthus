using Assets.Creature;
using System;
using TMPro;
using UnityEngine;

public enum AnimationType
{
    Idle, Running, Dead, Attack, Interact
}

public class CreatureRenderer : MonoBehaviour
{
    internal Animator Animator;
    internal CreatureData Data = new CreatureData();
    internal float RemainingTextDuration;
    private SpriteRenderer Highlight;
    private TextMeshPro Text;

    public void Awake()
    {
        Highlight?.gameObject.SetActive(false);
    }

    public void ShowText(string text, float duration)
    {
        if (Text != null)
        {
            Text.text = text;
            Text.color = ColorConstants.WhiteBase;

            RemainingTextDuration = duration;
        }
    }

    public void Start()
    {
        Highlight = Instantiate(Game.Instance.CreatureController.HightlightPrefab, transform);
        Highlight.gameObject.SetActive(false);
        Text = GetComponent<TextMeshPro>();
        if (Text == null)
        {
            Text = GetComponentInChildren<TextMeshPro>();
        }

        Animator = GetComponent<Animator>();
        if (Animator == null)
        {
            Animator = GetComponentInChildren<Animator>();
        }

        Data.Start();
    }

    public void Update()
    {
        UpdateFloatingText();

        if (Data.Update(Time.deltaTime))
        {
        }

        if (Data.Dead)
        {
            SetAnimation(AnimationType.Dead);
            
            Destroy(this);
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

    internal void SetAnimation(AnimationType animation)
    {
        if (Animator != null)
        {
            foreach (AnimationType animationState in Enum.GetValues(typeof(AnimationType)))
            {
                if (animationState == AnimationType.Idle)
                    continue;

                Animator.SetBool(animationState.ToString(), animationState == animation);
            }
        }
    }

    internal void UpdatePosition()
    {
        transform.position = new Vector3(Data.X, Data.Cell.Y, Data.Z);
        UpdateRotation();
    }

    internal void UpdateRotation()
    {
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
        try
        {
            if (Data.CurrentPathRequest.Ready())
            {
                Cell lastNode = null;
                foreach (var cell in Data.CurrentPathRequest.GetPath())
                {
                    if (lastNode != null)
                    {
                        Gizmos.DrawLine(lastNode.Vector + new Vector3(0, 1, 0), cell.Vector + new Vector3(0, 1, 0));
                    }
                    lastNode = cell;
                }
            }
        }
        catch 
        {
        }

        Gizmos.DrawCube(Map.Instance.GetCellAtCoordinate(Data.TargetCoordinate).Vector + new Vector3(0, 1, 0), new Vector3(0.1f, 0.1f, 0.1f));
    }

    private void UpdateFloatingText()
    {
        if (Text != null)
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
}