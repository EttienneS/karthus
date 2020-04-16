using UnityEngine;
using UnityEngine.UI;

public class ZoneInfoPanel : MonoBehaviour
{
    public InputField Name;
    public RoomPanel RoomPanel;
    public AreaPanel AreaPanel;
    public StoragePanel StoragePanel;
    internal ZoneBase CurrentZone;

    public void DeleteZone()
    {
        Game.Instance.ZoneController.Delete(CurrentZone);
        Destroy();
    }

    public void DoneEditing()
    {
        Game.Instance.Typing = false;
    }

    public void Destroy()
    {
        Destroy(gameObject);
        Game.Instance.Typing = false;
    }

    public void NameChanged()
    {
        Game.Instance.Typing = true;
        CurrentZone.Name = Name.text;
        Game.Instance.ZoneController.Refresh(CurrentZone);
    }

    internal void Show(ZoneBase selectedZone)
    {
        gameObject.SetActive(true);

        CurrentZone = selectedZone;

        RoomPanel.Hide();
        StoragePanel.Hide();
        AreaPanel.Hide();

        if (CurrentZone is RoomZone rz)
        {
            RoomPanel.Show(rz);
        }
        else if (CurrentZone is StorageZone sz)
        {
            StoragePanel.Show(sz);
        }
        else if (CurrentZone is AreaZone ar)
        {
            AreaPanel.Show(ar);
        }

        Name.text = CurrentZone.Name;
    }
}