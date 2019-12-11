using UnityEngine;

public partial class Game //.Instances
{
    private static CameraController _cameraController;
    private static CreatureController _creatureController;
    private static EntityInfoPanel _creatureInfoPanel;
    private static FactionController _factionController;
    private static FileController _fileController;
    private static Game _gameInstance;
    private static ItemController _itemController;
    private static LoadStatus _loadingPanel;
    private static MagicController _magicController;
    private static ManaDisplay _manaDisplay;
    private static Map _map;
    private static MaterialController _materialController;
    private static Minimap _minimap;
    private static OrderSelectionController _orderSelectionController;
    private static OrderTrayController _orderTrayController;
    private static PhysicsController _physicsController;
    private static SaveManager _saveManager;
    private static SpriteStore _spriteStore;
    private static StructureController _structureController;
    private static TimeManager _timeManager;
    private static UIController _uiController;
    private static VisualEffectController _visualEffectController;
    public static MapGenerator MapGenerator;

    public static CameraController CameraController
    {
        get
        {
            if (_cameraController == null)
            {
                _cameraController = GameObject.Find(ControllerConstants.CameraController).GetComponent<CameraController>();
            }

            return _cameraController;
        }
    }

    public static Game Controller
    {
        get
        {
            if (_gameInstance == null)
            {
                _gameInstance = GameObject.Find(ControllerConstants.GameController).GetComponent<Game>();
            }

            return _gameInstance;
        }
    }

    public static CreatureController CreatureController
    {
        get
        {
            if (_creatureController == null)
            {
                _creatureController = GameObject.Find(ControllerConstants.CreatureController).GetComponent<CreatureController>();
            }

            return _creatureController;
        }
    }

    public static EntityInfoPanel EntityInfoPanel
    {
        get
        {
            if (_creatureInfoPanel == null)
            {
                _creatureInfoPanel = GameObject.Find("EntityInfoPanel").GetComponent<EntityInfoPanel>();
            }

            return _creatureInfoPanel;
        }
    }

    public static FactionController FactionController
    {
        get
        {
            if (_factionController == null)
            {
                _factionController = GameObject.Find("FactionController").GetComponent<FactionController>();
            }

            return _factionController;
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
            if (_itemController == null)
            {
                _itemController = GameObject.Find("ItemController").GetComponent<ItemController>();
            }

            return _itemController;
        }
    }

    public static LoadStatus LoadingPanel
    {
        get
        {
            if (_loadingPanel == null)
            {
                _loadingPanel = GameObject.Find("LoadingPanel").GetComponent<LoadStatus>();
            }

            return _loadingPanel;
        }
    }

    public static MagicController MagicController
    {
        get
        {
            if (_magicController == null)
            {
                _magicController = GameObject.Find(ControllerConstants.MagicController).GetComponent<MagicController>();
            }

            return _magicController;
        }
    }

    public static ManaDisplay ManaDisplay
    {
        get
        {
            if (_manaDisplay == null)
            {
                _manaDisplay = GameObject.Find(ControllerConstants.ManaDisplay).GetComponent<ManaDisplay>();
            }

            return _manaDisplay;
        }
    }

    public static Map Map
    {
        get
        {
            if (_map == null)
            {
                _map = GameObject.Find(ControllerConstants.MapController).GetComponent<Map>();
            }

            return _map;
        }
    }

    public static MaterialController MaterialController
    {
        get
        {
            if (_materialController == null)
            {
                _materialController = GameObject.Find(ControllerConstants.MaterialController).GetComponent<MaterialController>();
            }

            return _materialController;
        }
    }

    public static Minimap Minimap
    {
        get
        {
            if (_minimap == null)
            {
                _minimap = GameObject.Find("Minimap").GetComponent<Minimap>();
            }

            return _minimap;
        }
    }

    public static OrderSelectionController OrderSelectionController
    {
        get
        {
            if (_orderSelectionController == null)
            {
                _orderSelectionController = GameObject.Find(ControllerConstants.OrderSelectionController).GetComponent<OrderSelectionController>();
            }

            return _orderSelectionController;
        }
    }

    public static OrderTrayController OrderTrayController
    {
        get
        {
            if (_orderTrayController == null)
            {
                _orderTrayController = GameObject.Find("OrderTray").GetComponent<OrderTrayController>();
            }

            return _orderTrayController;
        }
    }

    public static PhysicsController PhysicsController
    {
        get
        {
            if (_physicsController == null)
            {
                _physicsController = GameObject.Find(ControllerConstants.PhysicsController).GetComponent<PhysicsController>();
            }

            return _physicsController;
        }
    }

    public static SaveManager SaveManager
    {
        get
        {
            if (_saveManager == null)
            {
                _saveManager = GameObject.Find("SaveManager").GetComponent<SaveManager>();
            }

            return _saveManager;
        }
    }

    public static SpriteStore SpriteStore
    {
        get
        {
            if (_spriteStore == null)
            {
                _spriteStore = GameObject.Find(ControllerConstants.SpriteController).GetComponent<SpriteStore>();
            }

            return _spriteStore;
        }
    }

    public static StructureController StructureController
    {
        get
        {
            if (_structureController == null)
            {
                _structureController = GameObject.Find(ControllerConstants.StructureController).GetComponent<StructureController>();
            }

            return _structureController;
        }
    }

    public static TimeManager TimeManager
    {
        get
        {
            if (_timeManager == null)
            {
                _timeManager = GameObject.Find(ControllerConstants.TimeController).GetComponent<TimeManager>();
            }

            return _timeManager;
        }
    }

    public static UIController UIController
    {
        get
        {
            if (_uiController == null)
            {
                _uiController = GameObject.Find("UI").GetComponent<UIController>();
            }

            return _uiController;
        }
    }

    public static VisualEffectController VisualEffectController
    {
        get
        {
            if (_visualEffectController == null)
            {
                _visualEffectController = GameObject.Find(ControllerConstants.VisualEffectController).GetComponent<VisualEffectController>();
            }

            return _visualEffectController;
        }
    }
}