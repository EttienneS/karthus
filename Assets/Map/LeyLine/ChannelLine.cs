using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChannelLine : MonoBehaviour
{
    internal LineRenderer LineRenderer;
    internal Vector3 Source;
    internal Vector3 Target;
    internal int Intensity;
    internal float Duration;
    internal float Delta;
    internal List<Vector3> TargetPoints = new List<Vector3>();

    private void Awake()
    {
        LineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        Delta += Time.deltaTime;

        if (Delta < Duration)
        {
            var frac = Delta / Duration;

            var drawPoints = new List<Vector3>();
            drawPoints.Add(Target);
            drawPoints.Add(Source);

            for (int i = 0; i < TargetPoints.Count; i++)
            {
                drawPoints.Add(Source + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0));
                var trg = Vector3.Lerp(Target, TargetPoints[i], frac);
                drawPoints.Add(trg);
                drawPoints.Add(trg + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0));
                drawPoints.Add(trg);
                drawPoints.Add(trg + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0));
                drawPoints.Add(trg);
            }

            drawPoints.Add(Source);

            LineRenderer.positionCount = drawPoints.Count;
            LineRenderer.SetPositions(drawPoints.ToArray());
        }
        else
        {
            Destroy(this.gameObject);
        }
    }


    internal void SetProperties(Vector3 source, Vector3 target, int intensity, float duration, ManaColor manaColor)
    {
        Source = source;
        Target = target;
        Intensity = intensity;
        Duration = duration;

        TargetPoints = new List<Vector3>();

        for (int i = 0; i < intensity; i++)
        {
            TargetPoints.Add(target + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0));
        }

        LineRenderer.startColor = manaColor.GetActualColor();
        LineRenderer.endColor = LineRenderer.startColor;
        LineRenderer.material.SetColor("_EffectColor", manaColor.GetActualColor());

    }
}
