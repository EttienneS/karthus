using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum TimeStep
{
    Paused = 0,
    Normal = 1,
    Fast = 6,
    Hyper = 16
}

public class TimeData
{
    public int Hour;
    public int Minute;
}

public class TimeManager : MonoBehaviour
{
    public TimeData Data = new TimeData()
    {
        Hour = 6,
        Minute = 0
    };

    internal Dictionary<TimeStep, Button> AllButtons;
    internal Button FastButton;
    internal Button FasterButton;
    internal Button NormalButton;
    internal Button PauseButton;
    internal float TickInterval = 0.5f;
    internal Text TimeDisplay;
    internal float WorkInterval = 0.01f;
    internal float CombatInterval = 0.5f;

    private TimeStep _timeStep;

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

    public float MagicInterval = 0.01f;

    internal bool Paused
    {
        get
        {
            return TimeStep == TimeStep.Paused;
        }
    }

    public void Awake()
    {
        TimeDisplay = GetComponentsInChildren<Text>().First(t => t.name == "TimeDisplay");
        var buttons = GetComponentsInChildren<Button>();

        PauseButton = buttons.First(b => b.name == "PauseButton");
        //SlowButton = buttons.First(b => b.name == "SlowButton");
        NormalButton = buttons.First(b => b.name == "NormalButton");
        FastButton = buttons.First(b => b.name == "FastButton");
        FasterButton = buttons.First(b => b.name == "FasterButton");

        PauseButton.onClick.AddListener(() => { TimeStep = TimeStep.Paused; });
        NormalButton.onClick.AddListener(() => { TimeStep = TimeStep.Normal; });
        FastButton.onClick.AddListener(() => { TimeStep = TimeStep.Fast; });
        FasterButton.onClick.AddListener(() => { TimeStep = TimeStep.Hyper; });

        AllButtons = new Dictionary<TimeStep, Button>
        {
            { TimeStep.Paused,PauseButton},
            //{ TimeStep.Slow, SlowButton },
            { TimeStep.Normal, NormalButton},
            { TimeStep.Fast, FastButton },
            { TimeStep.Hyper, FasterButton }
        };
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
        }

        TimeDisplay.text = $"{Data.Hour.ToString().PadLeft(2, '0')}:{Data.Minute.ToString().PadLeft(2, '0')}";
    }

    internal void Pause()
    {
        TimeStep = TimeStep.Paused;
    }
}