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

    public void OrderClicked(Button btn, string structureName)
    {
        if (btn.image.color != Color.red)
        {
            btn.image.color = Color.red;
            selectedStructure = structureName;
        }
        else
        {
            btn.image.color = Color.white;
            selectedStructure = null;
        }
    }

    private void Start()
    {
        foreach (var structureData in StructureController.Instance.StructureDataReference.Values)
        {
            var button = Instantiate(OrderButtonPrefab, transform);
            button.onClick.AddListener(() => OrderClicked(button, structureData.Name));
            button.name = structureData.Name;
            button.image.sprite = StructureController.Instance.GetSpriteForStructure(structureData.Name);
            button.GetComponentInChildren<Text>().text = "Build " + structureData.Name;
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}