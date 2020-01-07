using UnityEngine;
using UnityEngine.UI;

public class ZoneInfoPanel : MonoBehaviour
{
    internal Zone CurrentZone;

    public InputField Name;
    public Text ZoneInfo;

    internal void Show(Zone selectedZone)
    {
        gameObject.SetActive(true);

        CurrentZone = selectedZone;

        Name.text = CurrentZone.Name;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        Game.Controller.Typing = false;
    }

    public void DeleteZone()
    {
        Game.ZoneController.Delete(CurrentZone);
        Hide();
    }

    public void NameChanged()
    {
        Game.Controller.Typing = true;
        CurrentZone.Name = Name.text;
        Game.ZoneController.Refresh(CurrentZone);
    }

    public void DoneEditing()
    {
        Game.Controller.Typing = false;
    }

    public void Update()
    {
        ZoneInfo.text = "Zone Info:\n\n";
        ZoneInfo.text += $"Cells: {CurrentZone.Cells.Count}\n\nStructures:\n\n";

        foreach (var structure in CurrentZone.Structures)
        {
            ZoneInfo.text += $"{structure.Name}";

            if (structure.IsContainer())
            {
                ZoneInfo.text += $": {structure.GetProperty(NamedProperties.ContainedItemType)} {structure.GetValue(NamedProperties.ContainedItemCount)}/{structure.GetValue(NamedProperties.Capacity)}";
            }

            ZoneInfo.text += "\n";
        }

        ZoneInfo.text += $"\nItems:\n\n";

        foreach (var item in CurrentZone.Items)
        {
            ZoneInfo.text = $"{item.Name}\n";
        }
    }
}