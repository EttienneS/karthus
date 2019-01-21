using UnityEngine;
using UnityEngine.UI;

public class OrderSelectionController : MonoBehaviour
{
    public OrderButton OrderButtonPrefab;

    private static OrderSelectionController _instance;

    private OrderButton BuildButton;

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

    public CellClickedDelegate CellClickOrder { get; set; }

    public void BuildClicked(string structureName)
    {
        BuildButton.Text = "Build " + structureName;

        CellClickOrder = cell =>
        {
            if (cell.Structure == null)
            {
                var blueprint = StructureController.Instance.GetStructureBluePrint(structureName);
                cell.AddContent(blueprint.gameObject);
                cell.Structure = blueprint;
                Taskmaster.Instance.AddTask(new Build(blueprint, cell));
            }
        };
    }

    public void BuildTypeClicked()
    {
        if (OrderTrayController.Instance.gameObject.activeInHierarchy)
        {
            OrderTrayController.Instance.gameObject.SetActive(false);
            BuildButton.Text = "Select Building";
            CellClickOrder = null;
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
                button.Button.onClick.AddListener(() => BuildClicked(structureData.Name));
                button.name = structureData.Name;
                button.Button.image.sprite = StructureController.Instance.GetSpriteForStructure(structureData.Name);

                if (structureData.Tiled)
                {
                    button.Button.image.type = Image.Type.Tiled;
                }

                button.Text = "Build " + structureData.Name;
            }
        }
    }

    private void Start()
    {
        OrderTrayController.Instance.gameObject.SetActive(false);

        BuildButton = Instantiate(OrderButtonPrefab, transform);
        BuildButton.Button.onClick.AddListener(BuildTypeClicked);
        BuildButton.Text = "Select Building";
    }

    // Update is called once per frame
    private void Update()
    {
    }
}