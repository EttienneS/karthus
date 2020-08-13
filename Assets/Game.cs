using Assets;
using Assets.Item;
using Assets.Models;
using Assets.Structures;
using Assets.UI;
using Assets.UI.TaskPanel;
using Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.Apple;
using Debug = UnityEngine.Debug;


public class Game : MonoBehaviour
{
    public CameraController CameraController;
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
    public MeshRendererFactory MeshRendererFactory;
    public OrderInfoPanel OrderInfoPanel;
    public OrderSelectionController OrderSelectionController;
    public OrderTrayController OrderTrayController;
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

    private static Game _instance;

    private readonly List<GameObject> _destroyCache = new List<GameObject>();
    private CreatureInfoPanel _currentCreatureInfoPanel;
    private ItemInfoPanel _currentItemInfoPanel;
    private StructureInfoPanel _currentStructureInfoPanel;
    private Tooltip _currentTooltip;
    private ZoneInfoPanel _currentZoneInfoPanel;
    private DateTime? _lastAutoSave = null;
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

    internal void HideTooltip()
    {
        if (_currentTooltip != null)
        {
            Destroy(_currentTooltip.gameObject);
        }
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
        if (SaveManager.SaveToLoad != null)
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

            SaveManager.SaveToLoad.Stores.ForEach(ZoneController.LoadStore);
            SaveManager.SaveToLoad.Rooms.ForEach(ZoneController.LoadRoom);
            SaveManager.SaveToLoad.Areas.ForEach(ZoneController.LoadArea);

            SaveManager.SaveToLoad.CameraData.Load(CameraController.Camera);
            SaveManager.SaveToLoad = null;
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
        Map.GenerateMap();
        FinalizeMap();
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

            CameraController.MoveToWorldCenter();

            MainMenuController.Toggle();
        }
    }

    private void Start()
    {
        UIController.Hide();

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

        Initialize();
    }

    internal void DestroyToolTip()
    {
        if (_currentTooltip != null)
        {
            _currentTooltip.Destroy();
        }
    }

    internal void DestroyCreaturePanel()
    {
        if (_currentCreatureInfoPanel != null)
        {
            _currentCreatureInfoPanel.Destroy();
        }
    }

    internal void DestroyItemInfoPanel()
    {
        if (_currentItemInfoPanel != null)
        {
            _currentItemInfoPanel.Destroy();
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

    internal void DestroyZonePanel()
    {
        if (_currentZoneInfoPanel != null)
        {
            _currentZoneInfoPanel.Destroy();
        }
    }

    internal void DestroyStructureInfoPanel()
    {
        if (_currentStructureInfoPanel != null)
        {
            _currentStructureInfoPanel.Destroy();
        }
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
        else if ((DateTime.Now - _lastAutoSave.Value).TotalSeconds > 600)
        {
            // autosave every two minutes
            _lastAutoSave = DateTime.Now;
            SaveManager.Save();
        }

        HotkeyHandler.HandleHotkeys();

        DestroyItemsInCache();
    }

    internal void ShowCreaturePanel(List<CreatureRenderer> selectedCreatures)
    {
        DestroyCreaturePanel();
        _currentCreatureInfoPanel = Instantiate(CreatureInfoPanelPrefab, UI.transform);
        _currentCreatureInfoPanel.Show(selectedCreatures.Select(c => c.Data).ToList());
    }

    internal void ShowZonePanel(ZoneBase zone)
    {
        DestroyZonePanel();
        _currentZoneInfoPanel = Instantiate(ZoneInfoPanelPrefab, UI.transform);
        _currentZoneInfoPanel.Show(zone);
    }

    internal void ShowItemPanel(List<ItemData> selectedItems)
    {
        DestroyItemInfoPanel();
        _currentItemInfoPanel = Instantiate(ItemInfoPanelPrefab, UI.transform);
        _currentItemInfoPanel.Show(selectedItems);
    }

    internal void ShowStructureInfoPanel(List<Structure> selectedStructures)
    {
        DestroyStructureInfoPanel();
        _currentStructureInfoPanel = Instantiate(StructureInfoPanelPrefab, UI.transform);
        _currentStructureInfoPanel.Show(selectedStructures.ToList());
    }
}
