using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private static TimeManager _instance;

    public static TimeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("TimeManager").GetComponent<TimeManager>();
            }

            return _instance;
        }
    }

    public void Start()
    {
        TimeStep = TimeStep.Normal;
    }

    private TimeStep _timeStep;
    public TimeStep TimeStep
    {
        get
        {
            return _timeStep;
        }
        set
        {
            _timeStep = value;
            if (_timeStep == TimeStep.Paused)
            {
                // camera and other systems run on fixedDeltaTime, ensure they are always set to something
                Time.timeScale = 0.000000001f;
                Time.fixedDeltaTime = 0.02f;
            }
            else
            {
                Time.timeScale = ((int)_timeStep) * 0.5f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }
        }
    }

}

public enum TimeStep
{
    Paused = 0,
    Slow = 1,
    Normal = 2,
    Fast = 4,
    Hyper = 8
}
