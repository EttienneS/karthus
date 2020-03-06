using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TimePanel : MonoBehaviour
{
    internal Dictionary<TimeStep, Button> AllButtons;
    public Button FastButton;
    public Button FasterButton;
    public Button NormalButton;
    public Button PauseButton;
    public Button SlowButton;
    public Text TimeDisplay;

    public void Start()
    {
        PauseButton.onClick.AddListener(() => Game.TimeManager.TimeStep = TimeStep.Paused);
        NormalButton.onClick.AddListener(() => Game.TimeManager.TimeStep = TimeStep.Normal);
        FastButton.onClick.AddListener(() => Game.TimeManager.TimeStep = TimeStep.Fast);
        FasterButton.onClick.AddListener(() => Game.TimeManager.TimeStep = TimeStep.Hyper);

        AllButtons = new Dictionary<TimeStep, Button>
        {
            { TimeStep.Paused, PauseButton},
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