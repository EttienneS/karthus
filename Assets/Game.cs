using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;

public enum DragMode
{
    SelectionRectangle, RepeatMouseSprite
}

public enum SelectionPreference
{
    Entity, Cell, Item, Structure, Creature
}

public partial class Game : MonoBehaviour
{
    public static float LoadProgress;
    public static bool Ready;
    public DragMode CurrentDragMode = DragMode.SelectionRectangle;
    public SpriteRenderer MouseSpriteRenderer;
    public Rotate RotateMouseLeft;
    public Rotate RotateMouseRight;
    public Vector3 SelectionEndScreen;
    public SelectionPreference SelectionPreference = SelectionPreference.Entity;
    public Vector3 SelectionStartWorld;
    public RectTransform selectSquareImage;

    public ValidateMouseSpriteDelegate ValidateMouse;
    internal static string LoadStatus;
    internal LineRenderer LineRenderer;
    internal List<Cell> SelectedCells = new List<Cell>();
    internal List<CreatureRenderer> SelectedCreatures = new List<CreatureRenderer>();
    internal List<Item> SelectedItems = new List<Item>();
    internal List<Structure> SelectedStructures = new List<Structure>();
    private bool _constructMode;
    private List<GameObject> _destroyCache = new List<GameObject>();
    private float _maxCurrentTime;
    private float _minCurrentTime;
    private TimeStep _oldTimeStep = TimeStep.Normal;
    private bool _shownOnce;

    public delegate void Rotate();

    public delegate bool ValidateMouseSpriteDelegate(IEnumerable<Cell> cells);

    public static bool Paused { get; set; }
    public float MaxTimeToClick { get; set; } = 0.60f;

    public float MinTimeToClick { get; set; } = 0.05f;

    public static void SetLoadStatus(string message, float progress)
    {
        LoadStatus = message;
        Debug.Log(LoadStatus);
        LoadProgress = progress;
    }

    public void AddItemToDestroy(GameObject gameObject)
    {
        lock (_destroyCache)
        {
            _destroyCache.Add(gameObject);
        }
    }

    public void AddLine(Cell start, Cell end)
    {
        LineRenderer.startColor = ColorConstants.InvalidColor;
        LineRenderer.endColor = ColorConstants.InvalidColor;

        LineRenderer.positionCount += 3;

        LineRenderer.SetPosition(LineRenderer.positionCount - 3, start.Vector);
        LineRenderer.SetPosition(LineRenderer.positionCount - 2, end.Vector);
        LineRenderer.SetPosition(LineRenderer.positionCount - 1, start.Vector);

        LineRenderer.startWidth = 0.1f;
        LineRenderer.endWidth = 0.1f;
    }

    public void ClearLine()
    {
        LineRenderer.positionCount = 0;
    }

    public void DeselectAll()
    {
        DeselectCreature();
        DeselectCell();
        DeselectStructure(true);
        DeselectItem();
    }

    public void DeselectCell()
    {
        SelectedCells.Clear();
    }

    public void DeselectCreature()
    {
        foreach (var creature in SelectedCreatures)
        {
            creature.DisableHightlight();
        }
        EntityInfoPanel.Hide();
        SelectedCreatures.Clear();
    }

    public void DeselectItem()
    {
        ClearLine();
        foreach (var item in SelectedItems)
        {
            item.HideOutline();
        }
        SelectedItems.Clear();
    }

    public void DeselectStructure(bool stopGhost)
    {
        if (stopGhost)
        {
            DisableMouseSprite();
        }

        ClearLine();
        foreach (var structure in SelectedStructures)
        {
            structure.HideOutline();
        }
        SelectedStructures.Clear();
    }

    public void DestroyItemsInCache()
    {
        try
        {
            lock (_destroyCache)
            {
                while (_destroyCache.Any())
                {
                    var item = _destroyCache[0];
                    _destroyCache.RemoveAt(0);
                    if (item != null)
                    {
                        Destroy(item);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Destroy failed: {ex}");
        }
    }

    public void DisableMouseSprite()
    {
        MouseSpriteRenderer.gameObject.SetActive(false);
        ValidateMouse = null;
        RotateMouseRight = null;
    }

    public void SetConstructSprite(Texture2D texture, int width, int height, ValidateMouseSpriteDelegate validation)
    {
        _constructMode = true;
        var mouseTex = texture.Clone();
        mouseTex.ScaleToGridSize(width, height);

        MouseSpriteRenderer.gameObject.SetActive(true);
        MouseSpriteRenderer.sprite = Sprite.Create(mouseTex,
                                                   new Rect(0, 0, width * Map.PixelsPerCell, height * Map.PixelsPerCell),
                                                   new Vector2(0, 0), Map.PixelsPerCell);

        ValidateMouse = validation;
    }

    public void SetMouseSprite(string spriteName, ValidateMouseSpriteDelegate validation)
    {
        _constructMode = false;
        MouseSpriteRenderer.gameObject.SetActive(true);
        MouseSpriteRenderer.sprite = SpriteStore.GetSprite(spriteName);

        ValidateMouse = validation;
    }

    private void HandleHotkeys()
    {
        if (Input.GetKeyDown("b"))
        {
            OrderSelectionController.BuildTypeClicked();
        }

        if (Input.GetKeyDown("n"))
        {
            OrderSelectionController.DesignateTypeClicked();
        }

        if (Input.GetKeyDown("e"))
        {
            RotateMouseRight?.Invoke();
        }

        if (Input.GetKeyDown("q"))
        {
            RotateMouseLeft?.Invoke();
        }

        if (Input.GetKeyDown("p"))
        {
            SaveManager.Save();
        }
        if (Input.GetKeyDown("["))
        {
            SaveManager.Load();
        }
    }

    private void HandleTimeControls()
    {
        if (Input.GetKeyDown("space"))
        {
            if (TimeManager.TimeStep == TimeStep.Paused)
            {
                TimeManager.TimeStep = _oldTimeStep;
            }
            else
            {
                _oldTimeStep = TimeManager.TimeStep;
                TimeManager.Pause();
            }
        }

        if (Input.GetKeyDown("1"))
        {
            TimeManager.TimeStep = TimeStep.Slow;
        }
        if (Input.GetKeyDown("2"))
        {
            TimeManager.TimeStep = TimeStep.Normal;
        }
        if (Input.GetKeyDown("3"))
        {
            TimeManager.TimeStep = TimeStep.Fast;
        }
        if (Input.GetKeyDown("4"))
        {
            TimeManager.TimeStep = TimeStep.Hyper;
        }
    }

    private void InitFactions()
    {
        foreach (var factionName in new[]
        {
            FactionConstants.Player,
            FactionConstants.Monster,
            FactionConstants.World
        })
        {
            var faction = new Faction
            {
                FactionName = factionName
            };
            FactionController.Factions.Add(factionName, faction);
        }
    }

    private bool MouseOverUi()
    {
        var overUI = Minimap.MouseInMinimapArea() || (EventSystem.current.IsPointerOverGameObject() && EventSystem.current.currentSelectedGameObject != null);

        if (overUI)
        {
            selectSquareImage.gameObject.SetActive(false);
        }

        return overUI;
    }

    private void MoveMouseSprite(Vector3 mousePosition)
    {
        if (MouseSpriteRenderer.gameObject.activeInHierarchy)
        {
            if (CurrentDragMode == DragMode.SelectionRectangle)
            {
                var cell = Map.GetCellAtPoint(Camera.main.ScreenToWorldPoint(mousePosition));

                MouseSpriteRenderer.transform.position = cell.Vector;

                float x = cell.X;
                float y = cell.Y;

                if (!_constructMode)
                {
                    x += 0.5f;
                    y += 0.5f;
                }
                MouseSpriteRenderer.transform.position = new Vector2(x, y);
            }
            else
            {
                //var end = Camera.main.ScreenToWorldPoint(mousePosition);

                var start = new Vector3(SelectionStartWorld.x - (MouseSpriteRenderer.size.x / 2), SelectionStartWorld.y - (MouseSpriteRenderer.size.y / 2));
                MouseSpriteRenderer.transform.position = start;
            }


            var cells = Map.GetRectangle(Map.GetCellAtCoordinate(Camera.main.ScreenToWorldPoint(SelectionEndScreen)), Map.GetCellAtCoordinate(SelectionStartWorld));
            if (ValidateMouse != null)
            {
                if (!ValidateMouse(cells))
                {
                    MouseSpriteRenderer.color = ColorConstants.InvalidColor;
                }
                else
                {
                    MouseSpriteRenderer.color = ColorConstants.BluePrintColor;
                }
            }
        }
    }

    private void SelectCell()
    {
        if (OrderSelectionController.CellClickOrder != null)
        {
            Debug.Log($"Clicked: {SelectedCells.Count}: {SelectedCells[0]}");
            OrderSelectionController.CellClickOrder.Invoke(SelectedCells);
            DeselectCell();
        }
    }

    private void SelectCreature()
    {
        DeselectCell();
        DeselectStructure(true);
        DeselectItem();

        foreach (var creature in SelectedCreatures)
        {
            creature.EnableHighlight(ColorConstants.InvalidColor);
        }

        EntityInfoPanel.Show(SelectedCreatures.Select(c => c.Data).ToList());
    }

    private void SelectItem()
    {
        DeselectCell();
        DeselectStructure(true);
        DeselectCreature();

        foreach (var item in SelectedItems)
        {
            item.ShowOutline();
        }

        EntityInfoPanel.Show(SelectedItems);
    }

    private void SelectStructure()
    {
        foreach (var structure in SelectedStructures)
        {
            structure.ShowOutline();
        }

        EntityInfoPanel.Show(SelectedStructures.ToList());
    }

    private void Start()
    {
        LoadingPanel.LoadingTextBox.text = "Initializing...";

        UIController.Hide();
        LoadingPanel.Show();

        LineRenderer = GetComponent<LineRenderer>();

        selectSquareImage.gameObject.SetActive(false);
        MouseSpriteRenderer.gameObject.SetActive(false);

        InitFactions();

        MapGenerator = new MapGenerator();
    }

    private void Update()
    {
        if (!Ready)
        {
            if (MapGenerator.Done)
            {
                Ready = true;
            }
            else if (!MapGenerator.Busy)
            {
                CameraController.Camera.transform.position = new Vector3(Map.Width / 2, Map.Height / 2, -1);
                CameraController.Camera.orthographicSize = Map.Width / 2;
                MapGenerator.Busy = true;
                StartCoroutine(MapGenerator.Work());
            }
            return;
        }
        else if (!_shownOnce)
        {
            UIController.Show();
            EntityInfoPanel.Hide();
            OrderSelectionController.DisableAndReset();
            LoadingPanel.Hide();

            _shownOnce = true;
            CameraController.Camera.orthographicSize = 10;
            CameraController.MoveToCell(FactionController.PlayerFaction.Creatures[0].Cell);
        }

        var mousePosition = Input.mousePosition;

        HandleHotkeys();
        HandleTimeControls();
        MoveMouseSprite(mousePosition);

        if (Input.GetMouseButton(1))
        {
            // right mouse deselect all
            DeselectAll();

            EntityInfoPanel.Hide();
            DisableMouseSprite();
            OrderSelectionController.DisableAndReset();
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (MouseOverUi())
                {
                    return;
                }
                SelectionStartWorld = Camera.main.ScreenToWorldPoint(mousePosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (MouseOverUi())
                {
                    return;
                }

                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)
                    && OrderSelectionController.CellClickOrder == null)
                {
                    DeselectAll();
                }

                selectSquareImage.gameObject.SetActive(false);

                var endPoint = Camera.main.ScreenToWorldPoint(SelectionEndScreen);

                var startX = Mathf.Clamp(Mathf.Min(SelectionStartWorld.x, endPoint.x), 0, Map.Width);
                var startY = Mathf.Clamp(Mathf.Min(SelectionStartWorld.y, endPoint.y), 0, Map.Height);
                var endX = Mathf.Clamp(Mathf.Max(SelectionStartWorld.x, endPoint.x), 0, Map.Width);
                var endY = Mathf.Clamp(Mathf.Max(SelectionStartWorld.y, endPoint.y), 0, Map.Height);

                if (startX == endX && startY == endY)
                {
                    var point = new Vector3(startX, endY);

                    var clickedCell = Map.GetCellAtPoint(point);
                    if (clickedCell != null)
                    {
                        SelectedCells.Add(clickedCell);
                        if (clickedCell.Structure != null)
                        {
                            SelectedStructures.Add(clickedCell.Structure);
                        }
                        else if (clickedCell.Floor != null)
                        {
                            SelectedStructures.Add(clickedCell.Floor);
                        }

                        SelectedItems.AddRange(clickedCell.Items);
                    }

                    var clickedCreature = CreatureController.GetCreatureAtPoint(point);
                    if (clickedCreature != null)
                        SelectedCreatures.Add(clickedCreature);
                }
                else
                {
                    var pollStep = 1f;

                    for (var selX = startX; selX < endX; selX += pollStep)
                    {
                        for (var selY = startY; selY < endY; selY += pollStep)
                        {
                            var point = new Vector3(selX, selY);

                            var clickedCell = Map.GetCellAtPoint(point);
                            if (clickedCell != null && !SelectedCells.Contains(clickedCell))
                            {
                                SelectedCells.Add(clickedCell);
                                if (clickedCell.Structure != null)
                                {
                                    SelectedStructures.Add(clickedCell.Structure);
                                }
                                SelectedItems.AddRange(clickedCell.Items);
                            }

                            var clickedCreature = CreatureController.GetCreatureAtPoint(point);
                            if (clickedCreature != null && !SelectedCreatures.Contains(clickedCreature))
                                SelectedCreatures.Add(clickedCreature);
                        }
                    }
                }

                switch (SelectionPreference)
                {
                    case SelectionPreference.Cell:
                        if (SelectedCells.Count > 0)
                        {
                            SelectCell();
                        }
                        break;

                    case SelectionPreference.Item:
                        if (SelectedCells.Count > 0)
                        {
                            SelectItem();
                        }
                        break;

                    case SelectionPreference.Structure:
                        if (SelectedCells.Count > 0)
                        {
                            SelectStructure();
                        }
                        break;

                    case SelectionPreference.Creature:
                        if (SelectedCells.Count > 0)
                        {
                            SelectCreature();
                        }
                        break;

                    case SelectionPreference.Entity:
                        if (SelectedCreatures.Count > 0)
                        {
                            SelectCreature();
                        }
                        if (SelectedStructures.Count > 0)
                        {
                            SelectStructure();
                        }
                        if (SelectedItems.Count > 0)
                        {
                            SelectItem();
                        }
                        break;
                }
            }

            if (Input.GetMouseButton(0))
            {
                if (MouseOverUi())
                {
                    return;
                }

                switch (CurrentDragMode)
                {
                    case DragMode.SelectionRectangle:
                        if (!selectSquareImage.gameObject.activeInHierarchy)
                        {
                            selectSquareImage.gameObject.SetActive(true);
                        }

                        SelectionEndScreen = mousePosition;

                        var start = Camera.main.WorldToScreenPoint(SelectionStartWorld);
                        start.z = 0f;

                        selectSquareImage.position = (start + SelectionEndScreen) / 2;

                        var sizeX = Mathf.Abs(start.x - SelectionEndScreen.x);
                        var sizeY = Mathf.Abs(start.y - SelectionEndScreen.y);

                        selectSquareImage.sizeDelta = new Vector2(sizeX, sizeY);
                        break;

                    case DragMode.RepeatMouseSprite:

                        SelectionEndScreen = mousePosition;

                        var worldEnd = Camera.main.ScreenToWorldPoint(SelectionEndScreen);

                        //var minX = Mathf.Min(worldStart.x, worldEnd.x);
                        //var maxX = Mathf.Max(worldStart.x, worldEnd.x);
                        //var minY = Mathf.Min(worldStart.y, worldEnd.y);
                        //var maxY = Mathf.Max(worldStart.y, worldEnd.y);
                        MouseSpriteRenderer.size = new Vector2(SelectionStartWorld.x - worldEnd.x, SelectionStartWorld.y - worldEnd.y);

                        break;
                }
            }
        }
        DestroyItemsInCache();
    }
}