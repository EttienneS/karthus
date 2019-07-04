using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public enum TimeStep
{
    Paused = 0,
    Slow = 1,
    Normal = 2,
    Fast = 8,
    Hyper = 16
}

public class TimeData
{
    public int Hour;
    public int Minute;
}

public class TimeManager : MonoBehaviour
{
    internal Text TimeDisplay;
    internal Button PauseButton;
    internal Button SlowButton;
    internal Button NormalButton;
    internal Button FastButton;
    internal Button FasterButton;

    internal Dictionary<TimeStep, Button> AllButtons;

    public void Awake()
    {
        TimeDisplay = GetComponentsInChildren<Text>().First(t => t.name == "TimeDisplay");
        var buttons = GetComponentsInChildren<Button>();

        PauseButton = buttons.First(b => b.name == "PauseButton");
        SlowButton = buttons.First(b => b.name == "SlowButton");
        NormalButton = buttons.First(b => b.name == "NormalButton");
        FastButton = buttons.First(b => b.name == "FastButton");
        FasterButton = buttons.First(b => b.name == "FasterButton");

        PauseButton.onClick.AddListener(() => { TimeStep = TimeStep.Paused; });
        SlowButton.onClick.AddListener(() => { TimeStep = TimeStep.Slow; });
        NormalButton.onClick.AddListener(() => { TimeStep = TimeStep.Normal; });
        FastButton.onClick.AddListener(() => { TimeStep = TimeStep.Fast; });
        FasterButton.onClick.AddListener(() => { TimeStep = TimeStep.Hyper; });

        AllButtons = new Dictionary<TimeStep, Button>
        {
            { TimeStep.Paused,PauseButton},
            { TimeStep.Slow, SlowButton },
            { TimeStep.Normal, NormalButton},
            { TimeStep.Fast, FastButton },
            { TimeStep.Hyper, FasterButton }
        };
    }

    public TimeData Data = new TimeData()
    {
        Hour = 6,
        Minute = 0
    };

    internal float TickInterval = 1f;
    private TimeStep _timeStep = TimeStep.Paused;

    private float _timeTicks;

    public string Now
    {
        get
        {
            return $"{Data.Hour}:{Data.Minute}:{_timeTicks}";
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

            foreach (var step in AllButtons)
            {
                step.Value.GetComponent<Image>().color = step.Key == value ? ColorConstants.InvalidColor : Color.white;
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

            Game.SunController.UpdateSun(Data.Hour, Data.Minute);
        }

        TimeDisplay.text = $"{Data.Hour.ToString().PadLeft(2, '0')}:{Data.Minute.ToString().PadLeft(2, '0')} {Game.SunController.State}";
    }
}