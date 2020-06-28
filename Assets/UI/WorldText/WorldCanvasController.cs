using UnityEngine;
using UnityEngine.UI;

public class WorldCanvasController : MonoBehaviour
{
    public Text WorldTextPrefab;
    public Canvas Canvas;
    public ChunkRenderer ChunkRenderer;

    private void Start()
    {
        var rect = Canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(Game.Instance.MapData.ChunkSize, Game.Instance.MapData.ChunkSize);
        transform.position = new Vector3(Game.Instance.MapData.ChunkSize / 2, 0, Game.Instance.MapData.ChunkSize / 2);

        //var startX = ChunkRenderer.Data.X * Game.Instance.MapData.ChunkSize;
        //var endX = startX + Game.Instance.MapData.ChunkSize;
        //var startZ = ChunkRenderer.Data.Z * Game.Instance.MapData.ChunkSize;
        //var endZ = startZ + Game.Instance.MapData.ChunkSize; 
        //for (var x = startX; x < endX; x += 10)
        //{
        //    for (var z = startZ; z < endZ; z += 10)
        //    {
        //        var cell = Game.Instance.Map.GetCellAtCoordinate(x, z);
        //        AddCellLabel(cell, $"{x}\n{z}");
        //    }
        //}
    }

    public void AddCellLabel(Cell cell, string text)
    {
        var label = Instantiate(WorldTextPrefab, transform);
        label.name = $"Label {cell.X}:{cell.Z}";
        label.text = text;
        label.transform.position = new Vector3(cell.Vector.x, Game.Instance.MapData.StructureLevel + 0.1f, cell.Vector.z);
    }
}