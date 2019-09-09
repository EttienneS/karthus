using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DataDisplay : MonoBehaviour
{
    internal Text Description;
    internal Image Image;
    internal Text Title;

    public delegate void Click();

    public event Click Clicked;

    public void Awake()
    {
        var textItems = GetComponentsInChildren<Text>();

        Title = textItems.First(t => t.name == "Title");
        Image = GetComponentsInChildren<Image>().First(t => t.name == "Image"); ;
        Description = textItems.First(t => t.name == "Description");
    }

    public void OnClick()
    {
        Clicked?.Invoke();
    }

    public void SetData(string title, string description, Sprite sprite)
    {
        Title.text = title;
        Description.text = description;
        Image.sprite = sprite;
    }

    internal void SetData(Structure structure)
    {
        SetData(structure.Name, structure.Name, Game.SpriteStore.GetSpriteByName(structure.SpriteName));
    }

    internal void SetData(Cell cell)
    {
        SetData(cell.ToString(), cell.ToString(), Game.SpriteStore.MapSpriteTypeDictionary[cell.CellType.ToString()]);
    }

    internal void SetData(TaskBase task)
    {
        SetData(task.GetType().Name, task.Message, task.AssignedEntity != null ?
                                                   task.Creature.CreatureRenderer.GetIcon() :
                                                   Game.SpriteStore.GetPlaceholder());

        if (task.Failed)
        {
            GetComponent<Image>().color = ColorConstants.InvalidColor;
        }
        else
        {
            GetComponent<Image>().color = Color.white;
        }
    }
}