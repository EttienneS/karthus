﻿using UnityEngine;
using UnityEngine.UI;

public class TimeButton : MonoBehaviour
{
    public TimeStep Step;

    public void Click()
    {
        TimeManager.Instance.TimeStep = Step;
    }

    public void Start()
    {
        GetComponentInChildren<Text>().text = Step.ToString();
        name = Step + " Button";
    }

    private void Update()
    {
        if (TimeManager.Instance.TimeStep == Step)
        {
            GetComponent<Image>().color = Color.red;
        }
        else
        {
            GetComponent<Image>().color = Color.white;
        }
    }
}