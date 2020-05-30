using Assets;
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
    public LoadPanel CurrentLoadPanel;
    public CursorController Cursor;
    public DeveloperConsole DeveloperConsole;
    public FactionController FactionController;
    public FileController FileController;
    public IdService IdService;
    public ItemController ItemController;
    public ItemInfoPanel ItemInfoPanelPrefab;
    public LoadPanel LoadPanelPrefab;
    public MainMenuController MainMenuController;
    public Map Map;
    public MapData MapData;
    public MapGenerator MapGenerator;
    public OrderInfoPanel OrderInfoPanel;
    public OrderSelectionController OrderSelectionController;
    public OrderTrayController OrderTrayController;
    public RectTransform selectSquareImage;
    public SpriteStore SpriteStore;
    public StructureController StructureController;
    public StructureInfoPanel StructureInfoPanelPrefab;
    public TaskPanel TaskPanel;
    public TimeManager TimeManager;
    public Tooltip TooltipPrefab;
    public GameObject UI;
    public UIController UIController;
    public VisualEffectController VisualEffectController;
    public ZoneController ZoneController;
    public ZoneInfoPanel ZoneInfoPanelPrefab;

    internal SelectionPreference LastSelection = SelectionPreference.Creature;

    internal Cell MouseOverCell;

    internal string MouseSpriteName;

    internal List<Cell> SelectedCells = new List<Cell>();

    internal List<CreatureRenderer> SelectedCreatures = new List<CreatureRenderer>();

    internal List<Item> SelectedItems = new List<Item>();

    internal List<Structure> SelectedStructures = new List<Structure>();

    internal Vector3 SelectionEndScreen;

    internal SelectionPreference SelectionPreference;

    internal Vector3 SelectionStartWorld;

    private static Game _instance;

    private CreatureInfoPanel _currentCreatureInfoPanel;

    private ItemInfoPanel _currentItemInfoPanel;

    private StructureInfoPanel _currentStructureInfoPanel;

    private Tooltip _currentTooltip;

    private ZoneInfoPanel _currentZoneInfoPanel;

    private List<GameObject> _destroyCache = new List<GameObject>();

    private DateTime? _lastAutoSave = null;

    private TimeStep _oldTimeStep = TimeStep.Normal;

    private bool _shownOnce;

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

    public int MaxSize => MapData.Size * MapData.ChunkSize;

    public bool Paused { get; set; }

    public bool Typing { get; set; }

    public void AddItemToDestroy(GameObject gameObject)
    {
        lock (_destroyCache)
        {
            _destroyCache.Add(gameObject);
        }
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

        if (_currentTooltip != null)
        {
            _currentTooltip.Destroy();
        }
        SelectedCreatures.Clear();
    }

    public void DeselectItem()
    {
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
            Cursor.Disable();
        }

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

    public List<Cell> GetSelectedCells(Vector3 worldStartPoint, Vector3 worldEndPoint)
    {
        var cells = new List<Cell>();

        var startX = Mathf.Clamp(Mathf.Min(worldStartPoint.x, worldEndPoint.x), Map.MinX, Map.MaxX);
        var endX = Mathf.Clamp(Mathf.Max(worldStartPoint.x, worldEndPoint.x), Map.MinX, Map.MaxX);

        var startZ = Mathf.Clamp(Mathf.Min(worldStartPoint.z, worldEndPoint.z), Map.MinZ, Map.MaxZ);
        var endZ = Mathf.Clamp(Mathf.Max(worldStartPoint.z, worldEndPoint.z), Map.MinX, Map.MaxZ);

        // not currently used
        var startY = Mathf.Min(worldStartPoint.y, worldEndPoint.y);
        var endY = Mathf.Max(worldStartPoint.y, worldEndPoint.y);

        if (startX == endX && startZ == endZ)
        {
            var point = new Vector3(startX, startY, startZ);

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
                for (var selZ = startZ; selZ < endZ; selZ += pollStep)
                {
                    var point = new Vector3(selX, startY, selZ);

                    cells.Add(Map.GetCellAtPoint(point));
                }
            }
        }

        return cells.Distinct().ToList();
    }

    public Vector3? GetWorldMousePosition()
    {
        var inputRay = Instance.CameraController.Camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(inputRay, out RaycastHit hit))
        {
            return hit.point;
        }
        return null;
    }

    public bool SelectCreature()
    {
        foreach (var creature in SelectedCreatures)
        {
            creature.EnableHighlight(ColorConstants.GreenAccent);
        }

        if (SelectedCreatures?.Count > 0)
        {
            DeselectCell();
            DeselectStructure(true);
            DeselectItem();
            DeselectZone();

            if (_currentCreatureInfoPanel != null)
            {
                _currentCreatureInfoPanel.Destroy();
            }
            _currentCreatureInfoPanel = Instantiate(CreatureInfoPanelPrefab, UI.transform);
            _currentCreatureInfoPanel.Show(SelectedCreatures.Select(c => c.Data).ToList());
            return true;
        }

        return false;
    }

    internal void HideTooltip()
    {
        if (_currentTooltip != null)
        {
            Destroy(_currentTooltip.gameObject);
        }
    }

    internal void SelectZone(ZoneBase zone)
    {
        DeselectCell();
        DeselectCreature();
        DeselectStructure(true);
        DeselectItem();

        if (_currentZoneInfoPanel != null)
        {
            _currentZoneInfoPanel.Destroy();
        }
        _currentZoneInfoPanel = Instantiate(ZoneInfoPanelPrefab, UI.transform);
        _currentZoneInfoPanel.Show(zone);
    }

    internal void ShowLoadPanel()
    {
        CurrentLoadPanel = Instantiate(LoadPanelPrefab, UI.transform);
    }

    internal Tooltip ShowTooltip(string tooltipTitle, string tooltipText)
    {
        _currentTooltip = Instantiate(TooltipPrefab, UI.transform);
        _currentTooltip.Load(tooltipTitle, tooltipText);
        return _currentTooltip;
    }

    private void FinalizeMap()
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
                foreach (var zone in SaveManager.SaveToLoad.Areas)
                {
                    ZoneController.Load(zone);
                }
            }

            SaveManager.SaveToLoad.CameraData.Load(CameraController.Camera);
            SaveManager.SaveToLoad = null;
        }
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
            Cursor.RotateRight?.Invoke();
        }
        else if (Input.GetKeyDown("q"))
        {
            Cursor.RotateLeft?.Invoke();
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
        MapGenerator.GenerateMap();
        FinalizeMap();
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

    private void OnFirstRun()
    {
        if (!_shownOnce)
        {
            UIController.Show();

            OrderSelectionController.DisableAndReset();
            TaskPanel.Hide();

            DeveloperConsole.gameObject.SetActive(false);

            _shownOnce = true;

            CameraController.transform.position = new Vector3((Instance.MapData.ChunkSize * Instance.MapData.Size) / 2,
                                                               20,
                                                              (Instance.MapData.ChunkSize * Instance.MapData.Size) / 2);

            MainMenuController.Toggle();
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

            if (_currentItemInfoPanel != null)
            {
                _currentItemInfoPanel.Destroy();
            }
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

            if (_currentStructureInfoPanel != null)
            {
                _currentStructureInfoPanel.Destroy();
            }
            _currentStructureInfoPanel = Instantiate(StructureInfoPanelPrefab, UI.transform);
            _currentStructureInfoPanel.Show(SelectedStructures.ToList());
            return true;
        }
        return false;
    }

    private void Start()
    {
        UIController.Hide();

        selectSquareImage.gameObject.SetActive(false);

        if (SaveManager.SaveToLoad != null)
        {
            MapData.Seed = SaveManager.SaveToLoad.Seed;
        }
        else
        {
            if (string.IsNullOrEmpty(MapData.Seed))
            {
                MapData.Seed = NameHelper.GetRandomName() + " " + NameHelper.GetRandomName();
            }
            InitFactions();
        }
        IdService = new IdService();
        MapGenerator = new MapGenerator();
        ConstructController = new ConstructController();

        Initialize();
    }

    private void Update()
    {
        OnFirstRun();

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

        HandleHotkeys();

        if (Input.GetMouseButton(1))
        {
            // right mouse deselect all
            DeselectAll();
            OrderSelectionController.DisableAndReset();
        }
        else
        {
            var mousePosition = GetWorldMousePosition();

            if (mousePosition != null)
            {
                UpdateMouseOverTooltip(mousePosition.Value);

                if (Input.GetMouseButtonDown(0))
                {
                    if (MouseOverUi())
                    {
                        return;
                    }
                    SelectionStartWorld = GetWorldMousePosition().Value;
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

                    SelectedCells = GetSelectedCells(SelectionStartWorld, GetWorldMousePosition().Value);

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

                    SelectionEndScreen = Input.mousePosition;
                    var start = Instance.CameraController.Camera.WorldToScreenPoint(SelectionStartWorld);
                    start.z = 0f;

                    selectSquareImage.position = (start + SelectionEndScreen) / 2;

                    var sizeX = Mathf.Abs(start.x - SelectionEndScreen.x);
                    var sizeY = Mathf.Abs(start.y - SelectionEndScreen.y);

                    selectSquareImage.sizeDelta = new Vector2(sizeX, sizeY);
                }
            }
        }
        DestroyItemsInCache();
    }

    private void UpdateMouseOverTooltip(Vector3 mousePosition)
    {
        if (!MouseOverUi())
        {
            MouseOverCell = Map.GetCellAtPoint(GetWorldMousePosition().Value);
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

                //WorldTooltip.Show(MouseOverCell.BiomeRegion.SpriteName, content, new Vector3(125, Screen.height - 100));
            }
        }
    }
}