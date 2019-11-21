using System.Collections.Generic;
using UnityEngine;

public class Voidshift : MonoBehaviour
{
    internal SpriteRenderer Renderer;
    public float Cycle = 30f;
    internal float TimeLeft = 30f;
    internal Color TargetColor;
    internal Color CurrentColor;
    internal Color IntermColor;
    internal List<Color> Colors = new List<Color>();

    private void Start()
    {
        Renderer = GetComponent<SpriteRenderer>();

        Colors.Add(ManaColor.Blue.GetActualColor());
        Colors.Add(ManaColor.Red.GetActualColor());
        Colors.Add(ManaColor.Green.GetActualColor());
        Colors.Add(ManaColor.White.GetActualColor());
        Colors.Add(ManaColor.Black.GetActualColor());

        CurrentColor = Colors.GetRandomItem();
        TargetColor = Colors.GetRandomItem();
        Colors.Remove(CurrentColor);
        Colors.Remove(TargetColor);

        Renderer.material.SetColor("_EffectColor", CurrentColor);
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0f, 0f, 0.01f));

        if (TimeLeft <= Time.deltaTime)
        {
            Renderer.material.SetColor("_EffectColor", TargetColor);

            Colors.Add(CurrentColor);
            CurrentColor = TargetColor;
            TargetColor = Colors.GetRandomItem();
            TimeLeft = Cycle;
        }
        else
        {
            IntermColor = Color.Lerp(IntermColor, TargetColor, Time.deltaTime / TimeLeft);
            Renderer.material.SetColor("_EffectColor", IntermColor);

            TimeLeft -= Time.deltaTime;
        }
    }
}