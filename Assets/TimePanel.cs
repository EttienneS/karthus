using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TimePanel : MonoBehaviour
{
    internal Dictionary<TimeStep, Button> AllButtons;
    internal Button FastButton;
    internal Button FasterButton;
    internal Button NormalButton;
    internal Button PauseButton;
    internal Button SlowButton;
    internal Text TimeDisplay;

    public void Awake()
    {
        TimeDisplay = GetComponentsInChildren<Text>().First(t => t.name == "TimeDisplay");
        var buttons = GetComponentsInChildren<Button>();

        PauseButton = buttons.First(b => b.name == "PauseButton");
        SlowButton = buttons.First(b => b.name == "SlowButton");
        NormalButton = buttons.First(b => b.name == "NormalButton");
        FastButton = buttons.First(b => b.name == "FastButton");
        FasterButton = buttons.First(b => b.name == "FasterButton");

        PauseButton.onClick.AddListener(() => Game.TimeManager.TimeStep = TimeStep.Paused);
        NormalButton.onClick.AddListener(() => Game.TimeManager.TimeStep = TimeStep.Normal);
        FastButton.onClick.AddListener(() => Game.TimeManager.TimeStep = TimeStep.Fast);
        FasterButton.onClick.AddListener(() => Game.TimeManager.TimeStep = TimeStep.Hyper);

        AllButtons = new Dictionary<TimeStep, Button>
        {
            { TimeStep.Paused,PauseButton},
            { TimeStep.Slow, SlowButton },
            { TimeStep.Normal, NormalButton},
            { TimeStep.Fast, FastButton },
            { TimeStep.Hyper, FasterButton }
        };
    }

    public void Update()
    {
        foreach (var step in AllButtons)
        {
            step.Value.GetComponent<Image>().color = step.Key == Game.TimeManager.TimeStep ? ColorConstants.InvalidColor : Color.white;
        }

        TimeDisplay.text = $"{Game.TimeManager.Data.Hour.ToString().PadLeft(2, '0')}:{Game.TimeManager.Data.Minute.ToString().PadLeft(2, '0')}";
    }
}