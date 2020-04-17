using Assets.UI.TaskPanel;
using Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;

public enum SelectionPreference
{
    Anything, Cell, Item, Structure, Creature, Zone
}

public class Game : MonoBehaviour
{
    public CameraController CameraController;
    public ConstructController ConstructController;
    public CreatureController CreatureController;
    public CreatureInfoPanel CreatureInfoPanelPrefab;
    public DeveloperConsole DeveloperConsole;
    public FactionController FactionController;
    public FileController FileController;
    public IdService IdService;
    public ItemController ItemController;
    public ItemInfoPanel ItemInfoPanelPrefab;
    public LoadStatus LoadingPanel;
    public LoadPanel LoadPanel;
    public MainMenuController MainMenuController;
    public Map Map;
    public MapGenerator MapGenerator;
    public SpriteRenderer MouseSpriteRenderer;
    public OrderInfoPanel OrderInfoPanel;
    public OrderSelectionController OrderSelectionController;
    public OrderTrayController OrderTrayController;
    public RectTransform selectSquareImage;
    public SpriteStore SpriteStore;
    public StructureController StructureController;
    public StructureInfoPanel StructureInfoPanelPrefab;
    public TaskPanel TaskPanel;
    public TimeManager TimeManager;
    public Tooltip Tooltip;
    public GameObject UI;
    public UIController UIController;
    public ValidateMouseSpriteDelegate ValidateMouse;
    public VisualEffectController VisualEffectController;
    public Tooltip WorldTooltip;
    public ZoneController ZoneController;
    public ZoneInfoPanel ZoneInfoPanelPrefab;
    internal SelectionPreference LastSelection = SelectionPreference.Creature;
    internal LineRenderer LineRenderer;
    internal float LoadProgress;
    internal string LoadStatus;
    internal Cell MouseOverCell;
    internal string MouseSpriteName;
    internal bool Ready;
    internal Rotate RotateMouseLeft;
    internal Rotate RotateMouseRight;
    internal List<Cell> SelectedCells = new List<Cell>();
    internal List<CreatureRenderer> SelectedCreatures = new List<CreatureRenderer>();
    internal List<Item> SelectedItems = new List<Item>();
    internal List<Structure> SelectedStructures = new List<Structure>();
    internal Vector3 SelectionEndScreen;
    internal SelectionPreference SelectionPreference;
    internal Vector3 SelectionStartWorld;
    private static Game _instance;

    private bool _constructMode;

    private CreatureInfoPanel _currentCreatureInfoPanel;
    private ItemInfoPanel _currentItemInfoPanel;
    private StructureInfoPanel _currentStructureInfoPanel;
    private ZoneInfoPanel _currentZoneInfoPanel;
    private List<GameObject> _destroyCache = new List<GameObject>();

    private bool _finalizationStarted;

    private List<VisualEffect> _ghostEffects = new List<VisualEffect>();

    private DateTime? _lastAutoSave = null;

    private TimeStep _oldTimeStep = TimeStep.Normal;

    private bool _shownOnce;

    public delegate void Rotate();

    public delegate bool ValidateMouseSpriteDelegate(Cell cell);

    public static Game Instance
    {
        get
        {
            return _instance != null ? _instance : (_instance = GameObject.Find("GameController").GetComponent<Game>());
        }
        set
        {
            _instance = value;
        }
    }

    public float MaxTimeToClick { get; set; } = 0.60f;
    public float MinTimeToClick { get; set; } = 0.05f;
    public bool Paused { get; set; }
    public bool Typing { get; set; }

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

    public void ClearGhostEffects()
    {
        foreach (var effect in _ghostEffects)
        {
            effect.DestroySelf();
        }
        _ghostEffects.Clear();
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
        DeselectZone();
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
        if (_currentCreatureInfoPanel != null)
        {
            _currentCreatureInfoPanel.Destroy();
        }
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
        if (_currentItemInfoPanel != null)
        {
            _currentItemInfoPanel.Destroy();
        }
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
        if (_currentStructureInfoPanel != null)
        {
            _currentStructureInfoPanel.Destroy();
        }
        SelectedStructures.Clear();
    }

    public void DeselectZone()
    {
        if (_currentZoneInfoPanel != null)
        {
            _currentZoneInfoPanel.Destroy();
        }
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
        MouseSpriteRenderer.size = Vector2.one;
        ValidateMouse = null;
        RotateMouseRight = null;
    }

    public List<Cell> GetSelectedCells(Vector3 worldStartPoint, Vector3 worldEndPoint)
    {
        var cells = new List<Cell>();

        var startX = Mathf.Clamp(Mathf.Min(worldStartPoint.x, worldEndPoint.x), Map.MinX, Map.MaxX);
        var startY = Mathf.Clamp(Mathf.Min(worldStartPoint.y, worldEndPoint.y), Map.MinY, Map.MaxY);
        var endX = Mathf.Clamp(Mathf.Max(worldStartPoint.x, worldEndPoint.x), Map.MinX, Map.MaxX);
        var endY = Mathf.Clamp(Mathf.Max(worldStartPoint.y, worldEndPoint.y), Map.MinX, Map.MaxY);

        if (startX == endX && startY == endY)
        {
            var point = new Vector3(startX, endY);

            var clickedCell = Map.GetCellAtPoint(point);
            if (clickedCell != null)
            {
                cells.Add(clickedCell);
            }
        }
        else
        {
            var pollStep = 1f;

            for (var selX = startX; selX < endX; selX += pollStep)
            {
                for (var selY = startY; selY < endY; selY += pollStep)
                {
                    var point = new Vector3(selX, selY);

                    cells.Add(Map.GetCellAtPoint(point));
                }
            }
        }

        return cells.Distinct().ToList();
    }

    public bool SelectCreature()
    {
        foreach (var creature in SelectedCreatures)
        {
            creature.EnableHighlight(ColorConstants.InvalidColor);
        }

        if (SelectedCreatures?.Count > 0)
        {
            DeselectCell();
            DeselectStructure(true);
            DeselectItem();
            DeselectZone();
            DeselectCreature();

            _currentCreatureInfoPanel = Instantiate(CreatureInfoPanelPrefab, UI.transform);
            _currentCreatureInfoPanel.Show(SelectedCreatures.Select(c => c.Data).ToList());
            return true;
        }

        return false;
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

    public void SetLoadStatus(string message, float progress)
    {
        LoadStatus = message;
        Debug.Log(LoadStatus);
        Instance.LoadProgress = progress;
    }

    public void SetMouseSprite(string spriteName, ValidateMouseSpriteDelegate validation)
    {
        _constructMode = false;
        MouseSpriteRenderer.gameObject.SetActive(true);
        MouseSpriteRenderer.sprite = SpriteStore.GetSprite(spriteName);
        MouseSpriteName = spriteName;
        ValidateMouse = validation;
    }

    internal void SelectZone(ZoneBase zone)
    {
        DeselectCell();
        DeselectCreature();
        DeselectStructure(true);
        DeselectItem();

        _currentZoneInfoPanel = Instantiate(ZoneInfoPanelPrefab, UI.transform);
        _currentZoneInfoPanel.Show(zone);
    }

    private void FinalizeStartup()
    {
        if (SaveManager.SaveToLoad == null)
        {
            MapGenerator.MakeFactionBootStrap(Game.Instance.FactionController.PlayerFaction);
            MapGenerator.SpawnCreatures();
        }
        else
        {
            TimeManager.Data = SaveManager.SaveToLoad.Time;
            foreach (var item in SaveManager.SaveToLoad.Items)
            {
                ItemController.SpawnItem(item);
            }

            foreach (var faction in SaveManager.SaveToLoad.Factions)
            {
                FactionController.Factions.Add(faction.FactionName, faction);

                foreach (var creature in faction.Creatures.ToList())
                {
                    CreatureController.SpawnCreature(creature, creature.Cell, faction);
                }

                foreach (var structure in faction.Structures.ToList())
                {
                    IdService.EnrollEntity(structure);

                    foreach (var effect in structure.LinkedVisualEffects)
                    {
                        VisualEffectController.Load(effect);
                    }
                }

                faction.LoadHomeCells();
            }

            if (SaveManager.SaveToLoad.Stores != null)
            {
                foreach (var zone in SaveManager.SaveToLoad.Stores)
                {
                    ZoneController.Load(zone);
                }
            }
            if (SaveManager.SaveToLoad.Rooms != null)
            {
                foreach (var zone in SaveManager.SaveToLoad.Rooms)
                {
                    ZoneController.Load(zone);
                }
            }
            if (SaveManager.SaveToLoad.Areas != null)
            {
                SetLoadStatus($"Loading Areas", 0.97f);
                foreach (var zone in SaveManager.SaveToLoad.Areas)
                {
                    ZoneController.Load(zone);
                }
            }

            SetLoadStatus($"Loading Camera", 0.99f);
            SaveManager.SaveToLoad.CameraData.Load(CameraController.Camera);
            SaveManager.SaveToLoad = null;
        }
        Ready = true;
    }

    private void HandleHotkeys()
    {
        if (Typing)
        {
            return;
        }

        if (Input.GetKeyDown("`"))
        {
            Game.Instance.DeveloperConsole.Toggle();
        }
        else if (Input.GetKeyDown("space"))
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
        else if (Input.GetKeyDown("escape"))
        {
            MainMenuController.Toggle();
        }
        else if (Input.GetKeyDown("1"))
        {
            TimeManager.TimeStep = TimeStep.Slow;
        }
        else if (Input.GetKeyDown("2"))
        {
            TimeManager.TimeStep = TimeStep.Normal;
        }
        else if (Input.GetKeyDown("3"))
        {
            TimeManager.TimeStep = TimeStep.Fast;
        }
        else if (Input.GetKeyDown("4"))
        {
            TimeManager.TimeStep = TimeStep.Hyper;
        }
        else if (Input.GetKeyDown("b"))
        {
            OrderSelectionController.BuildTypeClicked();
        }
        else if (Input.GetKeyDown("n"))
        {
            OrderSelectionController.DesignateTypeClicked();
        }
        else if (Input.GetKeyDown("z"))
        {
            OrderSelectionController.ZoneTypeClicked();
        }
        else if (Input.GetKeyDown("e"))
        {
            RotateMouseRight?.Invoke();
        }
        else if (Input.GetKeyDown("q"))
        {
            RotateMouseLeft?.Invoke();
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

    private void Initialize()
    {
        if (!Ready)
        {
            if (MapGenerator.Done && !_finalizationStarted)
            {
                _finalizationStarted = true;
                FinalizeStartup();
            }
            else
            {
                CameraController.Camera.transform.position = new Vector3(25, 25, -1);
                CameraController.Camera.orthographicSize = 5;
                Debug.Log("Start map gen");
                MapGenerator.Work();
                Debug.Log("Map gen complete");
            }
        }
        else if (!_shownOnce)
        {
            UIController.Show();

            OrderSelectionController.DisableAndReset();
            LoadingPanel.Hide();
            LoadPanel.Hide();
            Tooltip.Hide();
            WorldTooltip.Hide();
            TaskPanel.Hide();

            DeveloperConsole.gameObject.SetActive(false);

            _shownOnce = true;

            CameraController.Camera.orthographicSize = 10;
            CameraController.transform.position = FactionController.PlayerFaction.Creatures[0].Cell.Vector;

            MainMenuController.Toggle();
        }
    }

    private bool MouseOverUi()
    {
        if (EventSystem.current == null)
        {
            // event system not on yet
            return false;
        }
        var overUI = EventSystem.current.IsPointerOverGameObject() && EventSystem.current.currentSelectedGameObject != null;

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
            var cell = Map.GetCellAtPoint(Camera.main.ScreenToWorldPoint(mousePosition));

            if (cell == null)
            {
                return;
            }

            float x = cell.X;
            float y = cell.Y;

            if (!_constructMode)
            {
                x += 0.5f;
                y += 0.5f;
            }

            MouseSpriteRenderer.transform.position = new Vector2(x, y);

            if (ValidateMouse != null)
            {
                if (!ValidateMouse(cell))
                {
                    MouseSpriteRenderer.color = ColorConstants.InvalidColor;
                }
                else
                {
                    MouseSpriteRenderer.color = ColorConstants.BluePrintColor;
                }
            }

            if (SelectionStartWorld != Vector3.zero && !_constructMode)
            {
                MouseSpriteRenderer.color = new Color(0, 0, 0, 0);
                ClearGhostEffects();
                foreach (var c in GetSelectedCells(SelectionStartWorld, Camera.main.ScreenToWorldPoint(mousePosition)))
                {
                    var color = ColorConstants.BluePrintColor;
                    if (ValidateMouse != null && !ValidateMouse(c))
                    {
                        color = ColorConstants.InvalidColor;
                    }

                    _ghostEffects.Add(VisualEffectController.SpawnSpriteEffect(null, c.Vector, MouseSpriteName, float.MaxValue, color));
                }
            }
        }
    }

    private bool Select(ZoneBase selectedZone, SelectionPreference selection)
    {
        switch (selection)
        {
            case SelectionPreference.Anything:

                for (int i = 0; i < 5; i++)
                {
                    switch (LastSelection)
                    {
                        case SelectionPreference.Creature:
                            LastSelection = SelectionPreference.Structure;
                            break;

                        case SelectionPreference.Structure:
                            LastSelection = SelectionPreference.Item;
                            break;

                        case SelectionPreference.Item:
                            LastSelection = SelectionPreference.Zone;
                            break;

                        case SelectionPreference.Zone:
                            LastSelection = SelectionPreference.Creature;
                            break;
                    }
                    if (Select(selectedZone, LastSelection))
                    {
                        break;
                    }
                }
                break;

            case SelectionPreference.Cell:
                if (SelectedCells.Count > 0)
                {
                    SelectCell();
                    return true;
                }
                break;

            case SelectionPreference.Item:
                if (SelectedCells.Count > 0)
                {
                    return SelectItem();
                }
                break;

            case SelectionPreference.Structure:
                if (SelectedCells.Count > 0)
                {
                    return SelectStructure();
                }
                break;

            case SelectionPreference.Creature:
                if (SelectedCells.Count > 0)
                {
                    return SelectCreature();
                }
                break;

            case SelectionPreference.Zone:
                if (selectedZone != null)
                {
                    SelectZone(selectedZone);
                    return true;
                }
                break;
        }
        return false;
    }

    private void SelectCell()
    {
        if (OrderSelectionController.CellClickOrder != null)
        {
            //Debug.Log($"Clicked: {SelectedCells.Count}: {SelectedCells[0]}");
            OrderSelectionController.CellClickOrder.Invoke(SelectedCells);
            DeselectCell();
        }
    }

    private bool SelectItem()
    {
        foreach (var item in SelectedItems)
        {
            item.ShowOutline();
        }

        if (SelectedItems?.Count > 0)
        {
            DeselectCell();
            DeselectStructure(true);
            DeselectCreature();
            DeselectZone();

            _currentItemInfoPanel = Instantiate(ItemInfoPanelPrefab, UI.transform);
            _currentItemInfoPanel.Show(SelectedItems);
            return true;
        }
        return false;
    }

    private bool SelectStructure()
    {
        foreach (var structure in SelectedStructures)
        {
            structure.ShowOutline();
        }

        if (SelectedStructures?.Count > 0)
        {
            DeselectCell();
            DeselectItem();
            DeselectCreature();
            DeselectZone();

            _currentStructureInfoPanel = Instantiate(StructureInfoPanelPrefab, UI.transform);
            _currentStructureInfoPanel.Show(SelectedStructures.ToList());
            return true;
        }
        return false;
    }

    private void Start()
    {
        Debug.Log("Start game");
        LoadingPanel.LoadingTextBox.text = "Initializing...";

        UIController.Hide();
        LoadingPanel.Show();

        LineRenderer = GetComponent<LineRenderer>();
        MouseSpriteRenderer.size = Vector2.one;

        selectSquareImage.gameObject.SetActive(false);
        MouseSpriteRenderer.gameObject.SetActive(false);

        if (SaveManager.SaveToLoad != null)
        {
            Map.Seed = SaveManager.SaveToLoad.Seed;
        }
        else
        {
            // set initial bound values
            Map.MinX = Map.Origin.X;
            Map.MinY = Map.Origin.Y;
            Map.MaxX = Map.Origin.X + Map.ChunkSize;
            Map.MaxY = Map.Origin.Y + Map.ChunkSize;

            if (string.IsNullOrEmpty(Map.Seed))
            {
                Map.Seed = NameHelper.GetRandomName() + " " + NameHelper.GetRandomName();
            }
            InitFactions();
        }
        IdService = new IdService();
        MapGenerator = new MapGenerator();
        ConstructController = new ConstructController();
    }

    private void Update()
    {
        Initialize();

        if (MainMenuController.MainMenuActive)
        {
            return;
        }

        if (_lastAutoSave == null)
        {
            // make the first autosave actually happen 2 mins after the game starts not on the first call
            _lastAutoSave = DateTime.Now;
        }
        else if ((DateTime.Now - _lastAutoSave.Value).TotalSeconds > 120)
        {
            // autosave every two minutes
            _lastAutoSave = DateTime.Now;
            SaveManager.Save();
        }

        var mousePosition = Input.mousePosition;

        HandleHotkeys();
        MoveMouseSprite(mousePosition);

        if (Input.GetMouseButton(1))
        {
            // right mouse deselect all
            DeselectAll();
            DisableMouseSprite();
            OrderSelectionController.DisableAndReset();
        }
        else
        {
            UpdateMouseOverTooltip(mousePosition);

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

                if (!Input.GetKey(KeyCode.LeftShift)
                    && !Input.GetKey(KeyCode.RightShift)
                    && OrderSelectionController.CellClickOrder == null)
                {
                    DeselectAll();
                }

                selectSquareImage.gameObject.SetActive(false);

                SelectedCells = GetSelectedCells(SelectionStartWorld, Camera.main.ScreenToWorldPoint(mousePosition));

                ZoneBase selectedZone = null;
                foreach (var cell in SelectedCells)
                {
                    if (cell.Structure != null)
                    {
                        SelectedStructures.Add(cell.Structure);
                    }
                    SelectedItems.AddRange(cell.Items);
                    SelectedCreatures.AddRange(cell.Creatures.Select(c => c.CreatureRenderer));

                    var zone = Game.Instance.ZoneController.GetZoneForCell(cell);
                    if (zone != null)
                    {
                        selectedZone = zone;
                    }
                }

                Select(selectedZone, SelectionPreference);

                SelectionStartWorld = Vector3.zero;
                ClearGhostEffects();
            }

            if (Input.GetMouseButton(0))
            {
                if (MouseOverUi())
                {
                    return;
                }

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
            }
        }
        DestroyItemsInCache();
    }

    private void UpdateMouseOverTooltip(Vector3 mousePosition)
    {
        if (!MouseOverUi())
        {
            MouseOverCell = Map.GetCellAtPoint(Camera.main.ScreenToWorldPoint(mousePosition));
            if (MouseOverCell != null)
            {
                var content = "";

                if (MouseOverCell.Structure != null)
                {
                    content += MouseOverCell.Structure.Name + "\n";
                }
                if (MouseOverCell.Floor != null)
                {
                    content += MouseOverCell.Floor.Name + "\n";
                }
                var creatures = IdService.CreatureLookup.Where(c => c.Value.Cell == MouseOverCell).ToList();
                if (creatures.Count > 0)
                {
                    foreach (var creature in creatures)
                    {
                        content += creature.Value.Name + ",";
                    }
                    content = content.Trim(',') + "\n";
                }

                var items = IdService.ItemLookup.Where(c => c.Value.Cell == MouseOverCell).ToList();
                if (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        content += $"{item.Value.Name} x{item.Value.Amount},";
                    }
                    content = content.Trim(',') + "\n";
                }

                WorldTooltip.Show(MouseOverCell.BiomeRegion.SpriteName, content, new Vector3(125, Screen.height - 100));
            }
        }
    }
}