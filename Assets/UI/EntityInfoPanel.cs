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

    public List<IEntity> CurrentEntities;

    public void Update()
    {
        if (CurrentEntities != null)
        {
            Properties.text = string.Empty;

            if (CurrentEntities.Count == 1)
            {
                var currentEntity = CurrentEntities[0];

                CreatureName.text = currentEntity.Name;

                if (currentEntity is CreatureData creature)
                {
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
                    var structure = currentEntity as Structure;

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

                if (currentEntity.Task != null)
                {
                    Task.text = currentEntity.Task.ToString();
                }
                else
                {
                    Task.text = "Finding Task";
                }
            }
            else
            {
                CreatureName.text = $"{CurrentEntities.Count} entities";
                foreach (var entity in CurrentEntities)
                {
                    Properties.text += $"- {entity.Name}\n";
                }
                Task.text = "Various";
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
        var total = 0;
        foreach (var entity in CurrentEntities)
        {
            if (entity.ManaPool.ContainsKey(col))
            {
                total += entity.ManaPool[col].Total;
            }
        }

        if (total > 0)
        {
            text.text = total.ToString();
        }
        else
        {
            text.text = "--";
        }
    }

    private List<ImageButton> _contextButtons = new List<ImageButton>();

    public void Show(IEnumerable<IEntity> entities)
    {
        gameObject.SetActive(true);
        CurrentEntities = entities.ToList();

        _contextButtons.Clear();
        foreach (Transform child in ButtonPanel.transform)
        {
            Destroy(child.gameObject);
        }

        if (entities.First() is CreatureData)
        {
            var creatures = entities.OfType<CreatureData>();
            AddMoveButton(creatures);
            AddAttackButton(creatures);
        }
        else
        {
            AddRemoveStructureButton(entities.OfType<Structure>());
        }
    }

    private void AddRemoveStructureButton(IEnumerable<Structure> structures)
    {
        var btn = AddButton(OrderSelectionController.DefaultRemoveText, OrderSelectionController.DefaultRemoveImage);
        btn.SetOnClick(() =>
        {
            SetActiveButton(btn);

            foreach (var structure in structures)
            {
                if (structure.IsBluePrint)
                {
                    Game.StructureController.DestroyStructure(structure);
                }
                else
                {
                    if (FactionController.PlayerFaction.Tasks.OfType<RemoveStructure>().Any(t => t.Structure == structure))
                    {
                        Debug.Log("Structure already flagged to remove");
                    }
                    else
                    {
                        FactionController.PlayerFaction
                                         .AddTaskWithCellBadge(
                                                new RemoveStructure(structure),
                                                null,
                                                structure.Cell,
                                                OrderSelectionController.DefaultRemoveImage);
                    }
                }
            }
        });
    }

    private void AddMoveButton(IEnumerable<CreatureData> creatures)
    {
        var btn = AddButton(OrderSelectionController.MoveText, OrderSelectionController.MoveIcon);
        btn.SetOnClick(() =>
        {
            SetActiveButton(btn);
            Game.Controller.SelectionPreference = SelectionPreference.Cell;
            Game.Controller.SetMouseSprite(OrderSelectionController.MoveIcon, (c) => c.TravelCost > 0);

            Game.OrderSelectionController.CellClickOrder = cells =>
            {
                foreach (var creature in creatures)
                {
                    var cell = cells[0];

                    var faction = creature.GetFaction();
                    var task = faction.AddTaskWithCellBadge(new Move(cell), creature, cell, OrderSelectionController.MoveIcon);
                    faction.AssignTask(creature, task);

                    creature.Task.CancelTask();
                    creature.Task = task;
                }
            };
        });
    }

    private void AddAttackButton(IEnumerable<CreatureData> creatures)
    {
        var btn = AddButton(OrderSelectionController.AttackText, OrderSelectionController.AttackIcon);
        btn.SetOnClick(() =>
        {
            SetActiveButton(btn);

            Game.Controller.SelectionPreference = SelectionPreference.Cell;
            Game.Controller.SetMouseSprite(OrderSelectionController.AttackIcon,
                                            (c) => c.GetEnemyCreaturesOf(FactionConstants.Player).Any());

            Game.OrderSelectionController.CellClickOrder = cells =>
            {
                foreach (var creature in creatures)
                {
                    foreach (var cell in cells)
                    {
                        foreach (var enemy in cell.GetEnemyCreaturesOf(creature.FactionName))
                        {
                            FactionController.PlayerFaction
                                             .AddTaskWithEntityBadge(new ExecuteAttack(enemy, new FireBlast()),
                                                                     null,
                                                                     enemy,
                                                                     OrderSelectionController.AttackIcon);
                        }
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