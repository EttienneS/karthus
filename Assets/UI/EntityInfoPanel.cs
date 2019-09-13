using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EntityInfoPanel : MonoBehaviour
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

    public IEntity CurrentEntity;

    public void Update()
    {
        if (CurrentEntity != null)
        {
            Properties.text = string.Empty;

            var creature = CurrentEntity as CreatureData;
            if (creature != null)
            {
                CreatureName.text = creature.Name;
                foreach (var property in creature.ValueProperties)
                {
                    Properties.text += $"{property.Key}:\t{property.Value.ToString()}\n";
                }

                foreach (var property in creature.StringProperties)
                {
                    Properties.text += $"{property.Key}:\t{property.Value}\n";
                }

                if (creature.Task != null)
                {
                    Task.text = creature.Task.ToString();
                }
                else
                {
                    Task.text = "Finding Task";
                }
            }
            else
            {
                var structure = CurrentEntity as Structure;
                CreatureName.text = structure.Name;

                Properties.text += $"Type:\t{structure.StructureType}\n";

                foreach (var property in structure.Properties)
                {
                    Properties.text += $"{property.Key}:\t{property.Value}\n";
                }

                if (!string.IsNullOrEmpty(structure.InUseBy))
                {
                    Properties.text += $"In use by:\t{structure.InUseBy}\n";
                }
            }

            if (CurrentEntity.Task != null)
            {
                Task.text = CurrentEntity.Task.ToString();
            }
            else
            {
                Task.text = "Finding Task";
            }

            SetManaLabels();
        }
    }

    private void SetManaLabels()
    {
        SetManaLabel(RedLabel, ManaColor.Red);
        SetManaLabel(GreenLabel, ManaColor.Green);
        SetManaLabel(BlueLabel, ManaColor.Blue);
        SetManaLabel(BlackLabel, ManaColor.Black);
        SetManaLabel(WhiteLabel, ManaColor.White);
    }

    private void SetManaLabel(Text text, ManaColor col)
    {
        if (CurrentEntity.ManaPool.ContainsKey(col))
        {
            text.text = CurrentEntity.ManaPool[col].Total.ToString();
        }
        else
        {
            text.text = "--";
        }
    }

    public void Show(IEntity entity)
    {
        gameObject.SetActive(true);
        CurrentEntity = entity;
    }

    public void Cancel()
    {
        CurrentEntity.Task.CancelTask();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}