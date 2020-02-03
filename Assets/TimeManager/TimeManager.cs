using UnityEngine;

public enum TimeStep
{
    Paused = 0,
    Slow = 1,
    Normal = 4,
    Fast = 12,
    Hyper = 50
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

    public float MagicInterval = 0.01f;
    internal float CreatureTick = 0.1f;

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
                Game.Instance.Paused = true;
            }
            else
            {
                Time.timeScale = ((int)_timeStep) * 0.25f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                Game.Instance.Paused = false;
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

    public void Start()
    {
        TimeStep = TimeStep.Normal;
    }

    public void Update()
    {
        if (!Game.Instance.Ready)
        {
            return;
        }

        _timeTicks += Time.deltaTime;

        if (_timeTicks >= CreatureTick)
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
    }

    internal void Pause()
    {
        TimeStep = TimeStep.Paused;
    }
}