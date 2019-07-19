using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CraftingScreen : MonoBehaviour
{
    public DataDisplay DataPrefab;

    private Structure _craftSource;
    public GameObject SourcePanel;
    public GameObject OptionsPanel;
    public GameObject QueuePanel;

    public Text RequirementsText;
    public Image RecipeImage;

    public void Show(Structure craftSource)
    {
        _craftSource = craftSource;
        gameObject.SetActive(true);

        ClearPanel();

        var dataDisplay = Instantiate(DataPrefab, SourcePanel.transform);
        dataDisplay.SetData(craftSource);

        //var first = true;
        //foreach (var craftingTask in craftSource.Tasks.OfType<Craft>())
        //{
        //    if (first)
        //    {
        //        SetRecipe(craftingTask);
        //        first = false;
        //    }

        //    AddDisplay(OptionsPanel.transform, craftingTask).Clicked += () => SetRecipe(craftingTask);
        //}

        //foreach (Craft task in FactionManager.Factions[FactionConstants.Player].GetTaskByOriginator(_craftSource.GetGameId()))
        //{
        //    AddDisplay(QueuePanel.transform, task);
        //}

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
            //var task = new Craft(SelectedRecipe.OutputItemName,
            //                     SelectedRecipe.RequiredItemNames,
            //                     _craftSource.Coordinates,
            //                     SelectedRecipe.CraftTime)
            //{
            //    BusyEmote = SelectedRecipe.BusyEmote,
            //    DoneEmote = SelectedRecipe.DoneEmote
            //};

            //FactionManager.Factions[FactionConstants.Player].AddTask(task, _craftSource.GetGameId());
            //AddDisplay(QueuePanel.transform, SelectedRecipe);
        }

        Scale();
    }

    private void Scale()
    {
        OptionsPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _craftSource.Tasks.Count * 70f);
        QueuePanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, FactionController.PlayerFaction.GetTaskByOriginator(_craftSource.GetGameId()).Count() * 70f);

        //OptionsPanel.transform.position = new Vector2(0, 0);
        //QueuePanel.transform.position = new Vector2(0, 0);
    }

    //private DataDisplay AddDisplay(Transform parent, Craft recipe)
    //{
    //    var display = Instantiate(DataPrefab, parent);
    //    display.SetData(recipe.OutputItemName, recipe.OutputItemName, Game.SpriteStore.GetSpriteByName(recipe.OutputItemName));

    //    return display;
    //}

    //public void SetRecipe(Craft task)
    //{
    //    SelectedRecipe = task;
    //    RequirementsText.text = string.Empty;

    //    foreach (var item in task.RequiredItemNames)
    //    {
    //        RequirementsText.text += $"- {item}\n";
    //    }

    //    RecipeImage.sprite = Game.SpriteStore.GetSpriteByName(task.OutputItemName);
    //}

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}