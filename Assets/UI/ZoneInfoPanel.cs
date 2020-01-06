using UnityEngine;
using UnityEngine.UI;

public class ZoneInfoPanel : MonoBehaviour
{
    internal Zone CurrentZone;

    public InputField Name;

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
}
