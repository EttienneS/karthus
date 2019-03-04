using System.Linq;
using UnityEngine;

public class CraftingScreen : MonoBehaviour
{
    public DataDisplay DataPrefab;

    private static CraftingScreen _instance;

    private bool _firstRun = true;

    public GameObject SourcePanel;
    public GameObject OptionsPanel;

    public void Show(StructureData craftSource)
    {
        Instance.gameObject.SetActive(true);
        foreach (Transform child in SourcePanel.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in OptionsPanel.transform)
        {
            Destroy(child.gameObject);
        }

        var dataDisplay = Instantiate(DataPrefab, SourcePanel.transform);
        dataDisplay.SetData(craftSource);

        foreach (var craftingTask in craftSource.Tasks.OfType<Craft>())
        {
            var optionDisplay = Instantiate(DataPrefab, OptionsPanel.transform);
            optionDisplay.SetData(craftingTask.ItemType, craftingTask.ItemType, SpriteStore.Instance.GetSpriteByName(craftingTask.ItemType));
        }
        OptionsPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, craftSource.Tasks.Count * 70f);
        //OptionsPanel.transform.position = new Vector3(0)
    }

    public void Hide()
    {
        Instance.gameObject.SetActive(false);
    }

    public static CraftingScreen Instance
    {
        get
        {
            return _instance ?? (_instance = GameObject.Find("CraftingPanel").GetComponent<CraftingScreen>());
        }
    }
}