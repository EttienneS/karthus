using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    internal List<GameObject> UIControls;

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
}