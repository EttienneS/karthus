using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    private static TimeManager _instance;

    public Text TimeDisplay;

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

    private float _timeTicks;

    internal int Hour = 6;
    internal int Minute = 0;

    internal float TickInterval = 0.2f;



    public void Update()
    {
        //if (Paused) return;

        _timeTicks += Time.deltaTime;



        if (_timeTicks >= TickInterval)
        {
            _timeTicks = 0;
            Minute += 5;

            if (Minute >= 60)
            {
                Hour++;
                Minute = 0;

                if (Hour > 23)
                {
                    Hour = 0;
                }


            }

            SunController.Instance.UpdatePosition(Hour, Minute);
        }

        TimeDisplay.text = $"{Hour.ToString().PadLeft(2, '0')}:{Minute.ToString().PadLeft(2, '0')}";
    }

    public void Start()
    {
        TimeStep = TimeStep.Normal;
    }

    private TimeStep _timeStep;

    internal bool Paused
    {
        get
        {
            return TimeStep == TimeStep.Paused;
        }
    }

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