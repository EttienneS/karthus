using Assets.UI;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public FilterViewController FilterViewPrefab;
    internal List<GameObject> UIControls;

    private FilterViewController _currentFilterViewController;

    public void Awake()
    {
        UIControls = new List<GameObject>();
        foreach (Transform child in transform)
        {
            UIControls.Add(child.gameObject);
        }
    }

    public void Hide()
    {
        foreach (var child in UIControls)
        {
            child.SetActive(false);
        }
    }

    public void Show()
    {
        foreach (var child in UIControls)
        {
            child.SetActive(true);
        }
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
}