using Assets;
using Assets.Item;
using Assets.Models;
using Assets.ServiceLocator;
using Assets.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Game : MonoBehaviour, IGameService
{
    public CreatureInfoPanel CreatureInfoPanelPrefab;
    public ItemInfoPanel ItemInfoPanelPrefab;
    public LoadPanel LoadPanelPrefab;
    public MainMenuController MainMenuController;
    public MeshRendererFactory MeshRendererFactory;
    public OrderInfoPanel OrderInfoPanel;
    public OrderSelectionController OrderSelectionController;
    public OrderTrayController OrderTrayController;
    public StructureInfoPanel StructureInfoPanelPrefab;
    public Tooltip TooltipPrefab;
    public GameObject UI;
    public UIController UIController;
    public ZoneInfoPanel ZoneInfoPanelPrefab;
    internal static MapGenerationData MapGenerationData;
    internal LoadPanel CurrentLoadPanel;

    private readonly List<GameObject> _destroyCache = new List<GameObject>();
    private CreatureInfoPanel _currentCreatureInfoPanel;
    private ItemInfoPanel _currentItemInfoPanel;
    private StructureInfoPanel _currentStructureInfoPanel;
    private Tooltip _currentTooltip;
    private ZoneInfoPanel _currentZoneInfoPanel;
    private DateTime? _lastAutoSave = null;
    private bool _shownOnce;

    public int MaxSize => MapGenerationData.Size * MapGenerationData.ChunkSize;

    public bool Paused { get; set; }

    public bool Typing { get; set; }

    public void AddItemToDestroy(GameObject gameObject)
    {
        lock (_destroyCache)
        {
            _destroyCache.Add(gameObject);
        }
    }

    public void DestroyItemsInCache()
    {
        try
        {
            lock (_destroyCache)
            {
                while (_destroyCache.Count > 0)
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

    internal void DestroyStructureInfoPanel()
    {
        if (_currentStructureInfoPanel != null)
        {
            _currentStructureInfoPanel.Destroy();
        }
    }

    internal void DestroyToolTip()
    {
        if (_currentTooltip != null)
        {
            _currentTooltip.Destroy();
        }
    }

    internal void DestroyZonePanel()
    {
        if (_currentZoneInfoPanel != null)
        {
            _currentZoneInfoPanel.Destroy();
        }
    }

    internal void HideTooltip()
    {
        if (_currentTooltip != null)
        {
            Destroy(_currentTooltip.gameObject);
        }
    }

    internal void ShowCreaturePanel(List<CreatureRenderer> selectedCreatures)
    {
        DestroyCreaturePanel();
        _currentCreatureInfoPanel = Instantiate(CreatureInfoPanelPrefab, UI.transform);
        _currentCreatureInfoPanel.Show(selectedCreatures.Select(c => c.Data).ToList());
    }

    internal void ShowItemPanel(List<ItemData> selectedItems)
    {
        DestroyItemInfoPanel();
        _currentItemInfoPanel = Instantiate(ItemInfoPanelPrefab, UI.transform);
        _currentItemInfoPanel.Show(selectedItems);
    }

    internal void ShowLoadPanel()
    {
        CurrentLoadPanel = Instantiate(LoadPanelPrefab, UI.transform);
    }

    internal void ShowStructureInfoPanel(List<Structure> selectedStructures)
    {
        DestroyStructureInfoPanel();
        _currentStructureInfoPanel = Instantiate(StructureInfoPanelPrefab, UI.transform);
        _currentStructureInfoPanel.Show(selectedStructures.ToList());
    }

    internal Tooltip ShowTooltip(string tooltipTitle, string tooltipText)
    {
        _currentTooltip = Instantiate(TooltipPrefab, UI.transform);
        _currentTooltip.Load(tooltipTitle, tooltipText);
        return _currentTooltip;
    }

    internal void ShowZonePanel(ZoneBase zone)
    {
        DestroyZonePanel();
        _currentZoneInfoPanel = Instantiate(ZoneInfoPanelPrefab, UI.transform);
        _currentZoneInfoPanel.Show(zone);
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
            Loc.GetFactionController().Factions.Add(factionName, faction);
        }
    }

    private void OnFirstRun()
    {
        if (!_shownOnce)
        {
            UIController.Show();

            OrderSelectionController.DisableAndReset();

            _shownOnce = true;

            MainMenuController.Toggle();
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
            // make the first autosave actually happen 5 mins after the game starts not on the first call
            _lastAutoSave = DateTime.Now;
        }
        else if ((DateTime.Now - _lastAutoSave.Value).TotalSeconds > 600)
        {
            // autosave every two minutes
            _lastAutoSave = DateTime.Now;
            SaveManager.SaveGame();
        }

        HotkeyHandler.HandleHotkeys();

        DestroyItemsInCache();
    }

    public void Initialize()
    {
        UIController.Hide();

        if (SaveManager.SaveToLoad != null)
        {
            MapGenerationData = SaveManager.SaveToLoad.MapGenerationData;
        }
        else
        {
            if (MapGenerationData == null)
            {
                MapGenerationData = new MapGenerationData(NameHelper.GetRandomName() + " " + NameHelper.GetRandomName());
            }
            InitFactions();
        }

        Loc.GetMap().GenerateMap();
    }
}