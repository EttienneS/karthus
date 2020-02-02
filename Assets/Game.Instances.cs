using UnityEngine;

public partial class Game //.Instances
{
    public static MapGenerator MapGenerator;
    private static CameraController _cameraController;
    private static CreatureController _creatureController;
    private static EntityInfoPanel _entityInfoPanel;
    private static FactionController _factionController;
    private static FileController _fileController;
    private static Game _gameInstance;
    private static ItemController _itemController;
    private static LoadStatus _loadingPanel;
    private static MagicController _magicController;
    private static MainMenuController _mainMenuController;
    private static Map _map;
    private static MaterialController _materialController;
    private static Minimap _minimap;
    private static OrderInfoPanel _orderInfoPanel;
    private static OrderSelectionController _orderSelectionController;
    private static OrderTrayController _orderTrayController;
    private static PhysicsController _physicsController;
    private static SaveManager _saveManager;
    private static SpriteStore _spriteStore;
    private static StructureController _structureController;
    private static TimeManager _timeManager;
    private static UIController _uiController;
    private static VisualEffectController _visualEffectController;
    private static ZoneController _zoneController;
    private static ZoneInfoPanel _zoneInfoPanel;

    public static CameraController CameraController
    {
        get
        {
            return _cameraController ?? (_cameraController = GameObject.Find(ControllerConstants.CameraController).GetComponent<CameraController>());
        }
    }

    public static Game Controller
    {
        get
        {
            return _gameInstance ?? (_gameInstance = GameObject.Find(ControllerConstants.GameController).GetComponent<Game>());
        }
    }

    public static CreatureController CreatureController
    {
        get
        {
            return _creatureController ?? (_creatureController = GameObject.Find(ControllerConstants.CreatureController).GetComponent<CreatureController>());
        }
    }

    public static EntityInfoPanel EntityInfoPanel
    {
        get
        {
            return _entityInfoPanel ?? (_entityInfoPanel = GameObject.Find("EntityInfoPanel").GetComponent<EntityInfoPanel>());
        }
    }

    public static FactionController FactionController
    {
        get
        {
            return _factionController ?? (_factionController = GameObject.Find("FactionController").GetComponent<FactionController>());
        }
    }

    public static FileController FileController
    {
        get
        {
            if (_fileController == null)
            {
                _fileController = GameObject.Find("FileController").GetComponent<FileController>();
                _fileController.Load();
            }

            return _fileController;
        }
    }

    public static ItemController ItemController
    {
        get
        {
            return _itemController ?? (_itemController = GameObject.Find("ItemController").GetComponent<ItemController>());
        }
    }

    public static LoadStatus LoadingPanel
    {
        get
        {
            return _loadingPanel ?? (_loadingPanel = GameObject.Find("LoadingPanel").GetComponent<LoadStatus>());
        }
    }

    public static MagicController MagicController
    {
        get
        {
            return _magicController ?? (_magicController = GameObject.Find(ControllerConstants.MagicController).GetComponent<MagicController>());
        }
    }

    public static MainMenuController MainMenuController
    {
        get
        {
            return _mainMenuController ?? (_mainMenuController = GameObject.Find("MainMenu").GetComponent<MainMenuController>());
        }
    }

    public static Map Map
    {
        get
        {
            return _map ?? (_map = GameObject.Find(ControllerConstants.MapController).GetComponent<Map>());
        }
    }

    public static MaterialController MaterialController
    {
        get
        {
            return _materialController ?? (_materialController = GameObject.Find(ControllerConstants.MaterialController).GetComponent<MaterialController>());
        }
    }

    public static Minimap Minimap
    {
        get
        {
            return _minimap ?? (_minimap = GameObject.Find("Minimap").GetComponent<Minimap>());
        }
    }

    public static OrderInfoPanel OrderInfoPanel
    {
        get
        {
            return _orderInfoPanel ?? (_orderInfoPanel = GameObject.Find("OrderInfoPanel").GetComponent<OrderInfoPanel>());
        }
    }

    public static OrderSelectionController OrderSelectionController
    {
        get
        {
            return _orderSelectionController ?? (_orderSelectionController = GameObject.Find(ControllerConstants.OrderSelectionController).GetComponent<OrderSelectionController>());
        }
    }

    public static OrderTrayController OrderTrayController
    {
        get
        {
            return _orderTrayController ?? (_orderTrayController = GameObject.Find("OrderTray").GetComponent<OrderTrayController>());
        }
    }

    public static PhysicsController PhysicsController
    {
        get
        {
            return _physicsController ?? (_physicsController = GameObject.Find(ControllerConstants.PhysicsController).GetComponent<PhysicsController>());
        }
    }

    public static SaveManager SaveManager
    {
        get
        {
            return _saveManager ?? (_saveManager = GameObject.Find("SaveManager").GetComponent<SaveManager>());
        }
    }

    public static SpriteStore SpriteStore
    {
        get
        {
            return _spriteStore ?? (_spriteStore = GameObject.Find(ControllerConstants.SpriteController).GetComponent<SpriteStore>());
        }
    }

    public static StructureController StructureController
    {
        get
        {
            return _structureController ?? (_structureController = GameObject.Find(ControllerConstants.StructureController).GetComponent<StructureController>());
        }
    }

    public static TimeManager TimeManager
    {
        get
        {
            return _timeManager ?? (_timeManager = GameObject.Find(ControllerConstants.TimeController).GetComponent<TimeManager>());
        }
    }

    public static UIController UIController
    {
        get
        {
            return _uiController ?? (_uiController = GameObject.Find("UI").GetComponent<UIController>());
        }
    }

    public static VisualEffectController VisualEffectController
    {
        get
        {
            return _visualEffectController ?? (_visualEffectController = GameObject.Find(ControllerConstants.VisualEffectController).GetComponent<VisualEffectController>());
        }
    }

    public static ZoneController ZoneController
    {
        get
        {
            return _zoneController ?? (_zoneController = GameObject.Find("ZoneController").GetComponent<ZoneController>());
        }
    }

    public static ZoneInfoPanel ZoneInfoPanel
    {
        get
        {
            return _zoneInfoPanel ?? (_zoneInfoPanel = GameObject.Find("ZoneInfoPanel").GetComponent<ZoneInfoPanel>());
        }
    }
}