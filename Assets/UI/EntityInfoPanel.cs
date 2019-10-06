using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EntityInfoPanel : MonoBehaviour
{
    public Text CreatureName;
    public Text PropertiesPanel;
    public Text RedLabel;
    public Text GreenLabel;
    public Text BlueLabel;
    public Text WhiteLabel;
    public Text BlackLabel;
    public GameObject ButtonPanel;
    private List<ImageButton> _contextButtons = new List<ImageButton>();

    public ImageButton ImageButtonPrefab;
    public List<IEntity> CurrentEntities;

    public void Update()
    {
        if (CurrentEntities != null)
        {

            PropertiesPanel.text = string.Empty;

            if (CurrentEntities.Count == 1)
            {
                var currentEntity = CurrentEntities[0];

                CreatureName.text = currentEntity.Name;

                foreach (var property in currentEntity.ValueProperties)
                {
                    PropertiesPanel.text += $"{property.Key}:\t{property.Value.ToString()}\n";
                }

                foreach (var property in currentEntity.Properties)
                {
                    PropertiesPanel.text += $"{property.Key}:\t{property.Value}\n";
                }

                if (currentEntity is CreatureData creature)
                {
                    if (creature.Task != null)
                    {
                        if (string.IsNullOrWhiteSpace(creature.Task.Message))
                        {
                            Task.text = creature.Task.ToString();
                        }
                        else
                        {
                            Task.text = creature.Task.Message;
                        }
                    }
                    else
                    {
                        Task.text = "Finding Task";
                    }
                }
                else
                {
                    var structure = currentEntity as Structure;

                    PropertiesPanel.text += $"Rotation:\t {structure.Rotation}\n";

                    if (structure.IsBluePrint)
                    {
                        PropertiesPanel.text += "\n*Blueprint, waiting for construction...*\n";
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(structure.InUseBy))
                        {
                            PropertiesPanel.text += $"In use by:\t{structure.InUseBy}\n";
                        }

                        if (structure is Pipe pipe)
                        {
                            if (pipe.Attunement.HasValue)
                            {
                                PropertiesPanel.text += $"Attunment:\t{pipe.Attunement.Value}\n";
                            }
                            else
                            {
                                PropertiesPanel.text += $"Attunment:\tNone\n";
                            }
                        }

                        if (structure.Spell != null)
                        {
                            Task.text = structure.Spell.ToString();
                        }
                        else
                        {
                            Task.text = "--";
                        }
                    }

                }

                PropertiesPanel.text += $"\nLocation:\t{currentEntity.Cell}\n";
            }
            else
            {
                CreatureName.text = $"{CurrentEntities.Count} entities";
                foreach (var entity in CurrentEntities)
                {
                    PropertiesPanel.text += $"- {entity.Name}\n";
                }
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


    public void Show(IEnumerable<IEntity> entities)
    {
        gameObject.SetActive(true);
        CurrentEntities = entities.ToList();

        _contextButtons.Clear();
        foreach (Transform child in ButtonPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in PropertiesPanel.transform)
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
                    if (FactionController.PlayerFaction.Tasks.OfType<RemoveStructure>().Any(t => t.StructureToRemove == structure))
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
                            var task =  FactionController.PlayerFaction
                                             .AddTaskWithEntityBadge(new Interact(new ManaBlast(), enemy),
                                                                     null,
                                                                     enemy,
                                                                     OrderSelectionController.AttackIcon);

                            creature.Task?.CancelTask();
                            creature.Task = task;
                            break;
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