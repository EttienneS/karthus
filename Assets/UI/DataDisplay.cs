using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DataDisplay : MonoBehaviour
{
    internal Text Description;
    internal Image Image;
    internal Text Title;
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

    void Awake()
    {
        Title = GetComponentsInChildren<Text>().First(t => t.name == "Title");
        Description = GetComponentsInChildren<Text>().First(t => t.name == "Description");
        Image = GetComponentsInChildren<Image>().First(t => t.name == "Image");
    }
}