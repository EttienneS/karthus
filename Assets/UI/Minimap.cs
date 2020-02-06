﻿using UnityEngine;
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

    public int MinX
    {
        get
        {
            return (int)(Screen.width - ThisRect.sizeDelta.x - 15);
        }
    }

    public int MinY
    {
        get
        {
            return (int)(Screen.height - ThisRect.sizeDelta.y - 15);
        }
    }

    public bool MouseInMinimapArea()
    {
        var mouseX = Input.mousePosition.x;
        var mouseY = Input.mousePosition.y;
        return mouseX >= MinX && mouseX <= Screen.width &&
               mouseY >= MinY && mouseY <= Screen.height;
    }

    public void Click()
    {
        var x = (Game.Map.MaxX * ((Input.mousePosition.x - Screen.width + ThisRect.rect.width) / ThisRect.sizeDelta.x)) + 14;
        var y = (Game.Map.MaxY * ((Input.mousePosition.y - Screen.height + ThisRect.rect.height) / ThisRect.sizeDelta.y)) + 10;

        Game.CameraController.transform.position = new Vector3(x, y, Game.CameraController.Camera.transform.position.z);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Click();
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
        if (!Game.Instance.Ready)
        {
            return;
        }
        MinimapCamera.orthographicSize = (Game.Map.MaxX - Game.Map.MinX) / 2;
        MinimapCamera.transform.position = new Vector3(Game.Map.Center.X, Game.Map.Center.Y, -10);

        var relativeX = (ThisRect.sizeDelta.x * (Game.CameraController.transform.position.x / (Game.Map.MaxX - Game.Map.MinX))) - (ThisRect.sizeDelta.x / 2);
        var relativeY = (ThisRect.sizeDelta.y * (Game.CameraController.transform.position.y / (Game.Map.MaxY - Game.Map.MinY))) - (ThisRect.sizeDelta.y / 2);

        MapSquareImage.transform.localPosition = new Vector3(relativeX, relativeY, -10);

        var width = Mathf.CeilToInt(Game.CameraController.Camera.orthographicSize * Game.CameraController.RenderWidth);
        var height = Mathf.CeilToInt(Game.CameraController.Camera.orthographicSize * Game.CameraController.RenderHeight);

        MapSquareImage.sizeDelta = new Vector2(width, height);

        CoordText.text = Mathf.CeilToInt(Game.CameraController.Camera.transform.localPosition.x) + " : " +
                         Mathf.CeilToInt(Game.CameraController.Camera.transform.localPosition.y);
    }
}