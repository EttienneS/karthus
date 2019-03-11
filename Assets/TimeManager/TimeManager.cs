using UnityEngine;
using UnityEngine.UI;

public enum TimeStep
{
    Paused = 0,
    Slow = 1,
    Normal = 2,
    Fast = 4,
    Hyper = 8
}

public class TimeData
{
    public int Hour;
    public int Minute;
}

public class TimeManager : MonoBehaviour
{
    public Text TimeDisplay;

    public TimeData Data = new TimeData()
    {
        Hour = 6,
        Minute = 0
    };

    internal float TickInterval = 1f;
    private static TimeManager _instance;
    private TimeStep _timeStep;

    private float _timeTicks;

    public string Now
    {
        get
        {
            return $"{Data.Hour}:{Data.Minute}:{_timeTicks}";
        }
    }

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

    internal bool Paused
    {
        get
        {
            return TimeStep == TimeStep.Paused;
        }
    }

    internal void Pause()
    {
        TimeStep = TimeStep.Paused;
    }

    public void Start()
    {
        TimeStep = TimeStep.Normal;
    }

    public void Update()
    {
        //if (Paused) return;

        _timeTicks += Time.deltaTime;

        if (_timeTicks >= TickInterval)
        {
            _timeTicks = 0;
            Data.Minute += 5;

            if (Data.Minute >= 60)
            {
                Data.Hour++;
                Data.Minute = 0;

                if (Data.Hour > 23)
                {
                    Data.Hour = 0;
                }
            }

            SunController.Instance.UpdatePosition(Data.Hour, Data.Minute);
        }

        TimeDisplay.text = $"{Data.Hour.ToString().PadLeft(2, '0')}:{Data.Minute.ToString().PadLeft(2, '0')} {SunController.Instance.State}";
    }
}