using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Minimap : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    public Text CoordText;
    public RectTransform MapSquareImage;
    internal Camera MinimapCamera;
    internal RectTransform ThisRect;

    public void OnDrag(PointerEventData eventData)
    {
        OnPointerClick(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var x = (Game.Map.Width * ((Input.mousePosition.x - Screen.width + ThisRect.rect.width) / ThisRect.sizeDelta.x)) + 5;
        var y = (Game.Map.Height * (Input.mousePosition.y / ThisRect.sizeDelta.y)) - 5;

        Game.CameraController.transform.position = new Vector3(x, y, Game.CameraController.Camera.transform.position.z);

        Game.CameraController.UpdateCellsBasedOnCamera();
    }

    private void Start()
    {
        MinimapCamera = GameObject.Find(ControllerConstants.MinimapCamera).GetComponent<Camera>();
        ThisRect = GetComponent<RectTransform>();
        CoordText = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    private void Update()
    {
        MinimapCamera.orthographicSize = Game.Map.Width / 2;
        MinimapCamera.transform.position = new Vector3(Game.Map.Center.X, Game.Map.Center.Y, -10);

        var relativeX = (ThisRect.sizeDelta.x * (Game.CameraController.transform.position.x / Game.Map.Width)) - (ThisRect.sizeDelta.x / 2);
        var relativeY = (ThisRect.sizeDelta.y * (Game.CameraController.transform.position.y / Game.Map.Height)) - (ThisRect.sizeDelta.y / 2);

        MapSquareImage.transform.localPosition = new Vector3(relativeX, relativeY, -10);

        var width = Mathf.CeilToInt(Game.CameraController.Camera.orthographicSize * Game.CameraController.RenderWidth * 2);
        var height = Mathf.CeilToInt(Game.CameraController.Camera.orthographicSize * Game.CameraController.RenderHeight * 2);

        MapSquareImage.sizeDelta = new Vector2(width, height);

        CoordText.text = Mathf.CeilToInt(Game.CameraController.Camera.transform.localPosition.x) + " : " +
                         Mathf.CeilToInt(Game.CameraController.Camera.transform.localPosition.y);
    }
}