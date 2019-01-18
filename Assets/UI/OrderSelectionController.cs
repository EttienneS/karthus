using UnityEngine;
using UnityEngine.UI;

public class OrderSelectionController : MonoBehaviour
{
    public Button OrderButtonPrefab;

    public Structure selectedStructure;

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

    public void OrderClicked(Button btn, Structure structure)
    {
        if (btn.image.color != Color.red)
        {
            btn.image.color = Color.red;
            selectedStructure = structure;
        }
        else
        {
            btn.image.color = Color.white;
            selectedStructure = null;
        }
    }

    private void Start()
    {
        foreach (var structure in StructureController.Instance.AllStructures.Values)
        {
            var button = Instantiate(OrderButtonPrefab, transform);
            button.onClick.AddListener(() => OrderClicked(button, structure));
            button.name = structure.name;
            button.image.sprite = structure.SpriteRenderer.sprite;
            button.GetComponentInChildren<Text>().text = "Build " + structure.StructureData.Name;
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}