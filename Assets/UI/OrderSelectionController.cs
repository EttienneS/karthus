using UnityEngine;
using UnityEngine.UI;

public class OrderSelectionController : MonoBehaviour
{
    public Button OrderButtonPrefab;

    public string selectedStructure;

    private static OrderSelectionController _instance;

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

    public void BuildClicked(string structureName)
    {
        selectedStructure = structureName;
        BuildButton.GetComponentInChildren<Text>().text = "Build " + selectedStructure;
    }

    private Button BuildButton;

    private void Start()
    {
        OrderTrayController.Instance.gameObject.SetActive(false);

        BuildButton = Instantiate(OrderButtonPrefab, transform);
        BuildButton.onClick.AddListener(BuildTypeClicked);
        BuildButton.GetComponentInChildren<Text>().text = "Select Building";
    }

    public void BuildTypeClicked()
    {
        if (OrderTrayController.Instance.gameObject.activeInHierarchy)
        {
            OrderTrayController.Instance.gameObject.SetActive(false);
            BuildButton.GetComponentInChildren<Text>().text = "Select Building";
            selectedStructure = null;
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

    // Update is called once per frame
    private void Update()
    {
    }
}