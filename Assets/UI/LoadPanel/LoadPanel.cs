using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadPanel : MonoBehaviour
{
    public SaveInfo SaveInfoPrefab;
    public GameObject SavePanel;

    private List<SaveInfo> _infos;

    public void Start()
    {
        Load();
    }

    public void Hide()
    {
        Destroy(gameObject);
    }

    public void SetSelected(string path)
    {
        foreach (var info in _infos)
        {
            info.Deselect();
        }
        _selected = path;
    }

    public void LoadClicked()
    {
        SaveManager.Load(_selected);
    }

    private string _selected;

    public void Load()
    {
        _infos = new List<SaveInfo>();

        var counter = 1;
        foreach (var file in Directory.EnumerateFiles("Saves", "*.json", SearchOption.AllDirectories))
        {
            var info = Instantiate(SaveInfoPrefab, SavePanel.transform);
            info.LoadSave(file);
            _infos.Add(info);
            counter++;
        }

        var rt = SavePanel.GetComponent(typeof(RectTransform)) as RectTransform;
        
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, counter * SaveInfoPrefab.GetComponent<RectTransform>().sizeDelta.y);

    }
}
