using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CraftingScreen : MonoBehaviour
{
    public DataDisplay DataPrefab;

    private static CraftingScreen _instance;
    private StructureData _craftSource;
    public GameObject SourcePanel;
    public GameObject OptionsPanel;
    public GameObject QueuePanel;

    public Craft SelectedRecipe;

    public Text RequirementsText;
    public Image RecipeImage;

    public void Show(StructureData craftSource)
    {
        _craftSource = craftSource;
        Instance.gameObject.SetActive(true);

        ClearPanel();

        var dataDisplay = Instantiate(DataPrefab, SourcePanel.transform);
        dataDisplay.SetData(craftSource);

        var first = true;
        foreach (var craftingTask in craftSource.Tasks.OfType<Craft>())
        {
            if (first)
            {
                SetRecipe(craftingTask);
                first = false;
            }

            AddDisplay(OptionsPanel.transform, craftingTask).Clicked += () => SetRecipe(craftingTask);
        }

        foreach (Craft task in Taskmaster.Instance.GetTaskByOriginator(_craftSource.GetGameId()))
        {
            AddDisplay(QueuePanel.transform, task);
        }

        Scale();
    }

    private void ClearPanel()
    {
        foreach (Transform child in SourcePanel.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in OptionsPanel.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in QueuePanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void Make(int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            Taskmaster.Instance.AddTask(new Craft(SelectedRecipe.OutputItemType,
                                                  SelectedRecipe.RequiredItemTypes,
                                                  _craftSource.Coordinates), _craftSource.GetGameId());
            AddDisplay(QueuePanel.transform, SelectedRecipe);
        }

        Scale();
    }

    private void Scale()
    {
        OptionsPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _craftSource.Tasks.Count * 70f);
        QueuePanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Taskmaster.Instance.GetTaskByOriginator(_craftSource.GetGameId()).Count() * 70f);

        //OptionsPanel.transform.position = new Vector2(0, 0);
        //QueuePanel.transform.position = new Vector2(0, 0);
    }

    private DataDisplay AddDisplay(Transform parent, Craft recipe)
    {
        var display = Instantiate(DataPrefab, parent);
        display.SetData(recipe.OutputItemType, recipe.OutputItemType, SpriteStore.Instance.GetSpriteByName(recipe.OutputItemType));

        return display;
    }

    public void SetRecipe(Craft task)
    {
        SelectedRecipe = task;
        RequirementsText.text = string.Empty;

        foreach (var item in task.RequiredItemTypes)
        {
            RequirementsText.text += $"- {item}\n";
        }

        RecipeImage.sprite = SpriteStore.Instance.GetSpriteByName(task.OutputItemType);
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