using UnityEngine;
using UnityEngine.UI;

public class OrderSelectionController : MonoBehaviour
{
    public Button OrderButtonPrefab;

    private static OrderSelectionController _instance;

    private Button BuildButton;

    public delegate void CellClickedDelegate(Cell cell);

    public static OrderSelectionController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("OrderPanel").GetComponent<OrderSelectionController>();
            }

            return _instance;
        }
    }

    public CellClickedDelegate CellClicked { get; set; }

    public void BuildClicked(string structureName)
    {
        BuildButton.GetComponentInChildren<Text>().text = "Build " + structureName;

        CellClicked = cell =>
        {
            var blueprint = StructureController.Instance.GetStructureBluePrint(structureName);
            cell.AddContent(blueprint.gameObject);
            cell.Structure = blueprint;
            Taskmaster.Instance.AddTask(new Build(blueprint, cell));
        };
    }

    public void BuildTypeClicked()
    {
        if (OrderTrayController.Instance.gameObject.activeInHierarchy)
        {
            OrderTrayController.Instance.gameObject.SetActive(false);
            BuildButton.GetComponentInChildren<Text>().text = "Select Building";
            CellClicked = null;
        }
        else
        {
            OrderTrayController.Instance.gameObject.SetActive(true);

            foreach (Transform child in OrderTrayController.Instance.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var structureData in StructureController.Instance.StructureDataReference.Values)
            {
                var button = Instantiate(OrderButtonPrefab, OrderTrayController.Instance.transform);
                button.onClick.AddListener(() => BuildClicked(structureData.Name));
                button.name = structureData.Name;
                button.image.sprite = StructureController.Instance.GetSpriteForStructure(structureData.Name);

                if (structureData.Tiled)
                {
                    button.image.type = Image.Type.Tiled;
                }

                button.GetComponentInChildren<Text>().text = "Build " + structureData.Name;
            }
        }
    }

    private void Start()
    {
        OrderTrayController.Instance.gameObject.SetActive(false);

        BuildButton = Instantiate(OrderButtonPrefab, transform);
        BuildButton.onClick.AddListener(BuildTypeClicked);
        BuildButton.GetComponentInChildren<Text>().text = "Select Building";
    }

    // Update is called once per frame
    private void Update()
    {
    }
}