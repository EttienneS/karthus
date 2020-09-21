using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SaveInfo : MonoBehaviour
{
    public Text CreateDate;
    public Image Image;
    public Button This;
    public Text Title;
    private string _saveFile;

    public void LoadSave(string saveFile)
    {
        _saveFile = saveFile;

        var parts = saveFile.Split(new[] { '\\' });
        Title.text = parts[1];
        CreateDate.text = "Created: " + parts[2].Replace(".json", string.Empty).Replace("_", string.Empty);
        var imageFile = saveFile.Replace(".json", ".png");

        if (File.Exists(imageFile))
        {
            var tex = new Texture2D(0, 0);
            tex.LoadImage(File.ReadAllBytes(imageFile));
            Image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                              new Vector2(0, 0), MapController.PixelsPerCell);
        }
    }

    public void Select()
    {
        This.GetComponent<Image>().color = new Color(1, 0, 0, 0.3f);
    }

    public void Deselect()
    {
        This.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);
    }
}