using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CreatureInfoPanel : MonoBehaviour
{
    public Text CreatureName;
    public Text Properties;
    public Text Task;

    public Text RedLabel;
    public Text GreenLabel;
    public Text BlueLabel;
    public Text WhiteLabel;
    public Text BlackLabel;


    private bool _firstRun = true;

    public void Start()
    {
        if (_firstRun)
        {
            var children = GetComponentsInChildren<Text>().ToList();
            CreatureName = children.First(t => t.name == "CreatureName");
            Properties = children.First(t => t.name == "Properties");
            Task = children.First(t => t.name == "Task");

            RedLabel = children.First(t => t.name == "Red");
            GreenLabel = children.First(t => t.name == "Green");
            BlueLabel = children.First(t => t.name == "Blue");
            WhiteLabel = children.First(t => t.name == "White");
            BlackLabel = children.First(t => t.name == "Black");

            _firstRun = true;
        }
    }

    public CreatureRenderer CurrentCreature;

    public void Update()
    {
        if (CurrentCreature != null)
        {
            CreatureName.text = CurrentCreature.Data.Name;
            Properties.text = string.Empty;
            foreach (var property in CurrentCreature.Data.ValueProperties)
            {
                Properties.text += $"{property.Key}:\t{property.Value.ToString()}\n";
            }

            foreach (var property in CurrentCreature.Data.StringProperties)
            {
                Properties.text += $"{property.Key}:\t{property.Value}\n";
            }

            RedLabel.text = CurrentCreature.Data.ManaPool[ManaColor.Red].Total.ToString();
            GreenLabel.text = CurrentCreature.Data.ManaPool[ManaColor.Green].Total.ToString();
            BlueLabel.text = CurrentCreature.Data.ManaPool[ManaColor.Blue].Total.ToString();
            BlackLabel.text = CurrentCreature.Data.ManaPool[ManaColor.Black].Total.ToString();
            WhiteLabel.text = CurrentCreature.Data.ManaPool[ManaColor.White].Total.ToString();

            //Properties.text += $"\nMoving:\t{CurrentCreature.Data.Facing}";

            if (CurrentCreature.Data.Task != null)
            {
                Task.text = CurrentCreature.Data.Task.ToString();
            }
            else
            {
                Task.text = "Finding Task";
            }
        }
    }

    public void Show(CreatureRenderer creature)
    {
        gameObject.SetActive(true);
        CurrentCreature = creature;
    }

    public void Cancel()
    {
        CurrentCreature.Data.Task.CancelTask();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}