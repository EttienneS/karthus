using UnityEngine;
using UnityEngine.UI;

public class SaveInfo : MonoBehaviour
{
    public Text CreateDate;
    public Image Image;
    public Text PlayedTime;
    public Text Title;
    private string _saveFile;

    public void LoadSave(string saveFile)
    {
        _saveFile = saveFile;

        Title.text = saveFile;
    }

    public void Select()
    {
        Game.LoadPanel.SetSelected(_saveFile);
    }
}