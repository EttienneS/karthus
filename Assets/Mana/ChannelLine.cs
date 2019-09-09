using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChannelLine : MonoBehaviour
{
    internal LineRenderer LineRenderer;
    internal IEntity Source;
    internal IEntity Target;
    internal int Intensity;
    internal float Duration;
    internal float Delta;
    internal List<Vector3> TargetPoints = new List<Vector3>();

    private void Awake()
    {
        LineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        Delta += Time.deltaTime;

        if (Delta < Duration)
        {
            var frac = Delta / Duration;

            var srcPoint = Source.Cell.ToTopOfMapVector() + new Vector3(0, 0.4f);
            var trgPoint = Target.Cell.ToTopOfMapVector();

            var drawPoints = new List<Vector3>();
            drawPoints.Add(trgPoint);
            drawPoints.Add(srcPoint);

            for (int i = 0; i < TargetPoints.Count; i++)
            {
                drawPoints.Add(srcPoint + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0));
                var trg = Vector3.Lerp(trgPoint, trgPoint + TargetPoints[i], frac);
                drawPoints.Add(trg);
                drawPoints.Add(trg + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0));
                drawPoints.Add(trg);
                drawPoints.Add(trg + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0));
                drawPoints.Add(trg);
            }

            drawPoints.Add(srcPoint);

            LineRenderer.positionCount = drawPoints.Count;
            LineRenderer.SetPositions(drawPoints.ToArray());
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    internal void SetProperties(IEntity source, IEntity target, int intensity, float duration, ManaColor manaColor)
    {
        Source = source;
        Target = target;
        Intensity = intensity;
        Duration = duration;

        TargetPoints = new List<Vector3>();

        for (int i = 0; i < intensity; i++)
        {
            TargetPoints.Add(new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0));
        }

        LineRenderer.startColor = manaColor.GetActualColor();
        LineRenderer.endColor = LineRenderer.startColor;
        LineRenderer.material.SetColor("_EffectColor", manaColor.GetActualColor());
    }
}