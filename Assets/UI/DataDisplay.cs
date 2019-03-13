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
        SetData(structure.Name, structure.Name, SpriteStore.Instance.GetSpriteByName(structure.SpriteName));
    }

    internal void SetData(ItemData item)
    {
        SetData(item.Name, item.Name, SpriteStore.Instance.GetSpriteByName(item.SpriteName));
    }

    internal void SetData(CellData cell)
    {
        SetData(cell.Coordinates.ToString(), cell.Coordinates.ToString(), SpriteStore.Instance.MapSpriteTypeDictionary[cell.CellType.ToString()][0]);
    }

    internal void SetData(TaskBase task)
    {
        SetData(task.GetType().Name, task.ToString(), task.AssignedCreatureId > 0 ?
                                                        task.Creature.LinkedGameObject.FrontSprites[0] :
                                                        SpriteStore.Instance.GetPlaceholder());

        if (task.Failed)
        {
            GetComponent<Image>().color = Color.red;
        }
        else
        {
            GetComponent<Image>().color = Color.white;
        }
    }
}