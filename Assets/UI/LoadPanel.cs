using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadPanel : MonoBehaviour
{
    public SaveInfo SaveInfoPrefab;
    public GameObject SavePanel;

    private List<SaveInfo> _infos;

    public void Show()
    {
        gameObject.SetActive(true);
        Load();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetSelected(string path)
    {
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

        foreach (var file in Directory.EnumerateFiles("Saves", "*.json", SearchOption.AllDirectories))
        {
            var info = Instantiate(SaveInfoPrefab, SavePanel.transform);
            info.LoadSave(file);
            _infos.Add(info);
        }
    }
}
