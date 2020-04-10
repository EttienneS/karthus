using Assets.UI.TaskPanel;
using Structures;
using UI;
using UnityEngine;

public partial class Game //.Instances
{
    private static Game _instance;

    private CameraController _cameraController;
    private ConstructController _constructController;
    private CreatureController _creatureController;
    private CreatureInfoPanel _creatureInfoPanel;
    private DeveloperConsole _developerConsole;
    private FactionController _factionController;
    private FileController _fileController;
    private IdService _idService;
    private ItemController _itemController;
    private LoadStatus _loadingPanel;
    private LoadPanel _loadPanel;
    private MainMenuController _mainMenuController;
    private Map _map;
    private MapGenerator _mapGenerator;
    private OrderInfoPanel _orderInfoPanel;
    private OrderSelectionController _orderSelectionController;
    private OrderTrayController _orderTrayController;
    private PhysicsController _physicsController;
    private SpriteStore _spriteStore;
    private StructureController _structureController;
    private StructureInfoPanel _structureInfoPanel;
    private ItemInfoPanel _itemInfoPanel;
    private TimeManager _timeManager;
    private GameObject _ui;
    private UIController _uiController;
    private VisualEffectController _visualEffectController;
    private ZoneController _zoneController;
    private ZoneInfoPanel _zoneInfoPanel;
    private TaskPanel _taskPanel;

    public static TaskPanel TaskPanel
    {
        get
        {
            return Instance._taskPanel != null ? Instance._taskPanel : (Instance._taskPanel = GameObject.Find("TaskPanel").GetComponent<TaskPanel>());
        }
    }

    public static CameraController CameraController
    {
        get
        {
            return Instance._cameraController != null ? Instance._cameraController : (Instance._cameraController = GameObject.Find(ControllerConstants.CameraController).GetComponent<CameraController>());
        }
    }

    public static ConstructController ConstructController
    {
        get
        {
            return Instance._constructController;
        }
        set
        {
            Instance._constructController = value;
        }
    }

    public static CreatureController CreatureController
    {
        get
        {
            return Instance._creatureController != null ? Instance._creatureController : (Instance._creatureController = GameObject.Find(ControllerConstants.CreatureController).GetComponent<CreatureController>());
        }
    }

    public static CreatureInfoPanel CreatureInfoPanel
    {
        get
        {
            return Instance._creatureInfoPanel != null ? Instance._creatureInfoPanel : (Instance._creatureInfoPanel = GameObject.Find("CreatureInfoPanel").GetComponent<CreatureInfoPanel>());
        }
    }

    public static DeveloperConsole DeveloperConsole
    {
        get
        {
            return Instance._developerConsole != null ? Instance._developerConsole : (Instance._developerConsole = GameObject.Find(ControllerConstants.DeveloperConsole).GetComponent<DeveloperConsole>());
        }
    }
    public static FactionController FactionController
    {
        get
        {
            return Instance._factionController != null ? Instance._factionController : (Instance._factionController = GameObject.Find("FactionController").GetComponent<FactionController>());
        }
    }

    public static FileController FileController
    {
        get
        {
            if (Instance._fileController == null)
            {
                Instance._fileController = GameObject.Find("FileController").GetComponent<FileController>();
                Instance._fileController.Load();
            }

            return Instance._fileController;
        }
    }

    public static IdService IdService
    {
        get
        {
            return Instance._idService;
        }
        set
        {
            Instance._idService = value;
        }
    }

    public static Game Instance
    {
        get
        {
            return _instance != null ? _instance : (_instance = GameObject.Find(ControllerConstants.GameController).GetComponent<Game>());
        }
        set
        {
            _instance = value;
        }
    }

    public static ItemController ItemController
    {
        get
        {
            return Instance._itemController != null ? Instance._itemController : (Instance._itemController = GameObject.Find("ItemController").GetComponent<ItemController>());
        }
    }

    public static LoadStatus LoadingPanel
    {
        get
        {
            return Instance._loadingPanel != null ? Instance._loadingPanel : (Instance._loadingPanel = GameObject.Find("LoadingPanel").GetComponent<LoadStatus>());
        }
    }

    public static LoadPanel LoadPanel
    {
        get
        {
            return Instance._loadPanel != null ? Instance._loadPanel : (Instance._loadPanel = GameObject.Find(ControllerConstants.LoadPanel).GetComponent<LoadPanel>());
        }
    }

    public static MainMenuController MainMenuController
    {
        get
        {
            return Instance._mainMenuController != null ? Instance._mainMenuController : (Instance._mainMenuController = GameObject.Find("MainMenu").GetComponent<MainMenuController>());
        }
    }

    public static Map Map
    {
        get
        {
            return Instance._map != null ? Instance._map : (Instance._map = GameObject.Find(ControllerConstants.MapController).GetComponent<Map>());
        }
    }

    public static MapGenerator MapGenerator
    {
        get
        {
            return Instance._mapGenerator;
        }
        set
        {
            Instance._mapGenerator = value;
        }
    }

    public static OrderInfoPanel OrderInfoPanel
    {
        get
        {
            return Instance._orderInfoPanel != null ? Instance._orderInfoPanel : (Instance._orderInfoPanel = GameObject.Find("OrderInfoPanel").GetComponent<OrderInfoPanel>());
        }
    }

    public static OrderSelectionController OrderSelectionController
    {
        get
        {
            return Instance._orderSelectionController != null ? Instance._orderSelectionController : (Instance._orderSelectionController = GameObject.Find(ControllerConstants.OrderSelectionController).GetComponent<OrderSelectionController>());
        }
    }

    public static OrderTrayController OrderTrayController
    {
        get
        {
            return Instance._orderTrayController != null ? Instance._orderTrayController : (Instance._orderTrayController = GameObject.Find("OrderTray").GetComponent<OrderTrayController>());
        }
    }

    public static PhysicsController PhysicsController
    {
        get
        {
            return Instance._physicsController != null ? Instance._physicsController : (Instance._physicsController = GameObject.Find(ControllerConstants.PhysicsController).GetComponent<PhysicsController>());
        }
    }

    public static SpriteStore SpriteStore
    {
        get
        {
            return Instance._spriteStore != null ? Instance._spriteStore : (Instance._spriteStore = GameObject.Find(ControllerConstants.SpriteController).GetComponent<SpriteStore>());
        }
    }

    public static StructureController StructureController
    {
        get
        {
            return Instance._structureController != null ? Instance._structureController : (Instance._structureController = GameObject.Find(ControllerConstants.StructureController).GetComponent<StructureController>());
        }
    }

    public static StructureInfoPanel StructureInfoPanel
    {
        get
        {
            return Instance._structureInfoPanel != null ? Instance._structureInfoPanel : (Instance._structureInfoPanel = GameObject.Find("StructureInfoPanel").GetComponent<StructureInfoPanel>());
        }
    }

    public static ItemInfoPanel ItemInfoPanel
    {
        get
        {
            return Instance._itemInfoPanel != null ? Instance._itemInfoPanel : (Instance._itemInfoPanel = GameObject.Find("ItemInfoPanel").GetComponent<ItemInfoPanel>());
        }
    }

    public static TimeManager TimeManager
    {
        get
        {
            return Instance._timeManager != null ? Instance._timeManager : (Instance._timeManager = GameObject.Find(ControllerConstants.TimeController).GetComponent<TimeManager>());
        }
    }

    public static GameObject UI
    {
        get
        {
            return Instance._ui != null ? Instance._ui : (Instance._ui = GameObject.Find(ControllerConstants.UI));
        }
    }

    public static UIController UIController
    {
        get
        {
            return Instance._uiController != null ? Instance._uiController : (Instance._uiController = GameObject.Find("UI").GetComponent<UIController>());
        }
    }

    public static VisualEffectController VisualEffectController
    {
        get
        {
            return Instance._visualEffectController != null ? Instance._visualEffectController : (Instance._visualEffectController = GameObject.Find(ControllerConstants.VisualEffectController).GetComponent<VisualEffectController>());
        }
    }

    public static ZoneController ZoneController
    {
        get
        {
            return Instance._zoneController != null ? Instance._zoneController : (Instance._zoneController = GameObject.Find("ZoneController").GetComponent<ZoneController>());
        }
    }

    public static ZoneInfoPanel ZoneInfoPanel
    {
        get
        {
            return Instance._zoneInfoPanel != null ? Instance._zoneInfoPanel : (Instance._zoneInfoPanel = GameObject.Find("ZoneInfoPanel").GetComponent<ZoneInfoPanel>());
        }
    }
}