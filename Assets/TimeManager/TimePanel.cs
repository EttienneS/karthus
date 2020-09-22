using Assets.ServiceLocator;
using System.Collections.Generic;
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
        PauseButton.onClick.AddListener(() => Loc.GetTimeManager().SetTimeStep(TimeStep.Paused));
        SlowButton.onClick.AddListener(() => Loc.GetTimeManager().SetTimeStep(TimeStep.Slow));
        NormalButton.onClick.AddListener(() => Loc.GetTimeManager().SetTimeStep(TimeStep.Normal));
        FastButton.onClick.AddListener(() => Loc.GetTimeManager().SetTimeStep(TimeStep.Fast));
        FasterButton.onClick.AddListener(() => Loc.GetTimeManager().SetTimeStep(TimeStep.Hyper));

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
            step.Value.GetComponent<Image>().color = step.Key == Loc.GetTimeManager().GetTimeStep() ? ColorConstants.BlueBase : ColorConstants.GreyBase;
        }

        TimeDisplay.text = $"{Loc.GetTimeManager().Data.Hour.ToString().PadLeft(2, '0')}:{Loc.GetTimeManager().Data.Minute.ToString().PadLeft(2, '0')}";
    }
}