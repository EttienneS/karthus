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
    }

    private void Update()
    {
    }

    public void AddCellLabel(Cell cell, string text)
    {
        var label = Instantiate(WorldTextPrefab, transform);
        label.text = text;
        label.transform.position = cell.Vector;
    }
}