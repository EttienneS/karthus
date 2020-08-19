using Assets.UI;
using Assets.UI.TaskPanel;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public FilterViewController FilterViewPrefab;
    public TaskPanel TaskPanelPrefab;
    private FilterViewController _currentFilterViewController;
    private TaskPanel _currentTaskPanel;

    public void ToggleTaskpanel()
    {
        if (_currentTaskPanel == null)
        {
            _currentTaskPanel = Instantiate(TaskPanelPrefab, Game.Instance.UI.transform);
        }
        else
        {
            Destroy(_currentTaskPanel.gameObject);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void ShowFilterView(string title, List<FilterViewOption> options, OnOptionSelectedDelegate onOptionSelectedDelegate)
    {
        DestroyCurrentFilterViewController();

        _currentFilterViewController = Instantiate(FilterViewPrefab, transform);
        _currentFilterViewController.Load(title, options, onOptionSelectedDelegate);
    }

    private void DestroyCurrentFilterViewController()
    {
        if (_currentFilterViewController != null)
        {
            Destroy(_currentFilterViewController.gameObject);
        }
    }

    internal void Toggle()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }

    internal void ReloadTaskPanel()
    {
        if (_currentTaskPanel != null)
        {
            _currentTaskPanel.Reload();
        }
    }
}