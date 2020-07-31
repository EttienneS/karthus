using Assets.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public FilterViewController FilterViewPrefab;
    private FilterViewController _currentFilterViewController;


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
}