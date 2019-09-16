using System.Collections.Generic;
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
    public GameObject ButtonPanel;

    private bool _firstRun = true;

    public ImageButton ImageButtonPrefab;

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

    private List<ImageButton> _contextButtons = new List<ImageButton>();

    public void Show(IEntity entity)
    {
        gameObject.SetActive(true);
        CurrentEntity = entity;

        _contextButtons.Clear();
        foreach (Transform child in ButtonPanel.transform)
        {
            Destroy(child.gameObject);
        }

        if (entity is CreatureData creature)
        {
            AddMoveButton(creature);
            AddAttackButton(creature);
        }
        else
        {
            var structure = entity as Structure;
            Instantiate(ImageButtonPrefab, ButtonPanel.transform).SetText("Hammer");
            Instantiate(ImageButtonPrefab, ButtonPanel.transform).SetText("Time");
        }
    }

    private void AddMoveButton(CreatureData creature)
    {
        var btn = AddButton(OrderSelectionController.MoveText, OrderSelectionController.MoveIcon);
        btn.SetOnClick(() =>
        {
            SetActiveButton(btn);

            Game.Controller.SelectionPreference = SelectionPreference.Cell;
            Game.Controller.SetMouseSprite(OrderSelectionController.MoveIcon, (c) => c.TravelCost > 0);
            Game.OrderSelectionController.CellClickOrder = cells =>
            {
                var cell = cells[0];

                var faction = creature.GetFaction();
                var task = faction.AddTaskWithCellBadge(new Move(cell), creature, cell, OrderSelectionController.MoveIcon);
                faction.AssignTask(creature, task);

                creature.Task.CancelTask();
                creature.Task = task;
            };
        });
    }

    private void AddAttackButton(CreatureData creature)
    {
        var btn = AddButton(OrderSelectionController.AttackText, OrderSelectionController.AttackIcon);
        btn.SetOnClick(() =>
        {
            SetActiveButton(btn);

            Game.Controller.SelectionPreference = SelectionPreference.Cell;
            Game.Controller.SetMouseSprite(OrderSelectionController.AttackIcon, (c) => c.GetEnemyCreaturesOf(creature.FactionName).Any());

            Game.OrderSelectionController.CellClickOrder = cells =>
            {
                foreach (var cell in cells)
                {
                    foreach (var enemy in cell.GetEnemyCreaturesOf(creature.FactionName))
                    {
                        FactionController.PlayerFaction
                                         .AddTaskWithEntityBadge(new ExecuteAttack(enemy, new FireBlast()),
                                                                 null,
                                                                 creature,
                                                                 OrderSelectionController.AttackIcon);
                    }
                }
            };
        });
    }

    private void SetActiveButton(ImageButton btn)
    {
        foreach (var button in _contextButtons)
        {
            button.Image.color = Color.white;
        }
        btn.Button.image.color = Color.red;
    }

    public ImageButton AddButton(string title, string spriteName)
    {
        var button = Instantiate(ImageButtonPrefab, ButtonPanel.transform);
        button.SetText(title);
        button.SetImage(Game.SpriteStore.GetSprite(spriteName));

        _contextButtons.Add(button);

        return button;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}