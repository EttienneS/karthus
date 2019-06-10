using UnityEngine;
using UnityEngine.UI;

public class DataDisplay : MonoBehaviour
{
    public Text Description;
    public Image Image;
    public Text Title;

    public delegate void Click();

    public event Click Clicked;

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

    internal void SetData(StructureData structure)
    {
        SetData(structure.Name, structure.Name, Game.SpriteStore.GetSpriteByName(structure.SpriteName));
    }

    internal void SetData(CellData cell)
    {
        SetData(cell.Coordinates.ToString(), cell.Coordinates.ToString(), Game.SpriteStore.MapSpriteTypeDictionary[cell.CellType.ToString()][0]);
    }

    internal void SetData(TaskBase task)
    {
        SetData(task.GetType().Name, task.Message, task.AssignedCreatureId > 0 ?
                                                   task.Creature.LinkedGameObject.CreatureSprite.GetIcon() :
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