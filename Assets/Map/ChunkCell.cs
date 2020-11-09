using UnityEngine;

public class ChunkCell
{

    public ChunkCell(float height, Color color)
    {
        Height = height;
        Color = color;
    }

    public float Height { get; set; }

    public Color Color { get; set; }
}