using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CreatureInfoPanel : MonoBehaviour
{
    public GameObject ButtonPanel;
    public Text CreatureName;
    public List<Creature> CurrentCreatures;
    public Toggle FirstPanelToggle;
    public ImageButton ImageButtonPrefab;
    public Text Log;
    public Text PropertiesPanel;
    public GameObject TabPanel;
    private List<ImageButton> _contextButtons = new List<ImageButton>();

    public ImageButton AddButton(string spriteName)
    {
        var button = Instantiate(ImageButtonPrefab, ButtonPanel.transform);
        button.SetImage(Game.Instance.SpriteStore.GetSprite(spriteName));

        _contextButtons.Add(button);
        button.SetOnClick(() => SetActiveButton(button));
        return button;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void Show(IEnumerable<Creature> entities)
    {
        gameObject.SetActive(true);

        // switch to overview panel
        FirstPanelToggle.isOn = true;

        CurrentCreatures = entities.ToList();

        _contextButtons.Clear();
        foreach (Transform child in ButtonPanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in PropertiesPanel.transform)
        {
            Destroy(child.gameObject);
        }

        if (entities.First() is Creature creature && creature.IsPlayerControlled())
        {
            // creatures
            var creatures = entities.OfType<Creature>();

            AddButton(OrderSelectionController.MoveIcon).SetOnClick(() => MoveClicked(creatures));
            AddButton(OrderSelectionController.AttackIcon).SetOnClick(() => AttackClicked(creatures));
            AddButton(OrderSelectionController.DefaultRemoveIcon).SetOnClick(() =>
            {
                foreach (var c in creatures)
                {
                    if (c.InCombat)
                    {
                        c.Combatants.Clear();
                    }
                    c.AbandonTask();
                }
            });
        }
    }

    public void Update()
    {
        if (CurrentCreatures != null)
        {
            PropertiesPanel.text = string.Empty;
            TabPanel.SetActive(false);

            if (CurrentCreatures.Count == 1)
            {
                var currentEntity = CurrentCreatures[0];

                Log.text = string.Empty;

                CreatureName.text = currentEntity.Name;

                foreach (var property in currentEntity.ValueProperties)
                {
                    PropertiesPanel.text += $"{property.Key}:\t{property.Value.ToString("N0")}\n";
                }

                foreach (var property in currentEntity.Properties)
                {
                    PropertiesPanel.text += $"{property.Key}:\t{property.Value}\n";
                }

                if (currentEntity is Creature creature)
                {
                    if (creature.IsPlayerControlled())
                    {
                        TabPanel.SetActive(true);

                        foreach (var line in creature.LogHistory)
                        {
                            Log.text += $"{line}\n";
                        }

                        PropertiesPanel.text += $"\nMood: {creature.MoodString} ({creature.Mood})\n";

                        foreach (var feeling in creature.Feelings)
                        {
                            PropertiesPanel.text += $"\t{feeling}\n";
                        }

                        LogTask(creature);
                    }

                    PropertiesPanel.text += $"\nLocation:\t{creature.X:F1}:{creature.Z:F1}\n\n";
                }
            }
            else
            {
                CreatureName.text = $"{CurrentCreatures.Count} creatures";
            }
        }
    }

    private void AttackClicked(IEnumerable<Creature> creatures)
    {
        Game.Instance.SelectionPreference = SelectionPreference.Cell;
        Game.Instance.Cursor.SetSprite(Game.Instance.SpriteStore.GetSprite(OrderSelectionController.AttackIcon),
                                        (cell) => cell.GetEnemyCreaturesOf(FactionConstants.Player).Any());

        Game.Instance.OrderSelectionController.CellClickOrder = cells =>
        {
            foreach (var creature in creatures)
            {
                foreach (var cell in cells)
                {
                    foreach (var enemy in cell.GetEnemyCreaturesOf(creature.FactionName))
                    {
                        creature.Combatants.Add(enemy);
                        break;
                    }
                }
            }
        };
    }

    private void LogTask(Creature creature)
    {
        if (creature.Task != null)
        {
            if (string.IsNullOrWhiteSpace(creature.Task.Message))
            {
                PropertiesPanel.text += $"Task: \t{creature.Task}\n";
            }
            else
            {
                PropertiesPanel.text += $"Task: \t{creature.Task.Message}\n";
            }
        }
        else
        {
            PropertiesPanel.text += "Finding Task\n";
        }
    }

    private void MoveClicked(IEnumerable<Creature> creatures)
    {
        Game.Instance.SelectionPreference = SelectionPreference.Cell;
        Game.Instance.Cursor.SetSprite(Game.Instance.SpriteStore.GetSprite(OrderSelectionController.MoveIcon), (cell) => cell.TravelCost > 0);

        Game.Instance.OrderSelectionController.CellClickOrder = cells =>
        {
            foreach (var creature in creatures)
            {
                if (creature.InCombat)
                {
                    creature.Combatants.Clear();
                }
                var cell = cells[0];

                var faction = creature.GetFaction();
                var task = new Move(cell);
                task.AddCellBadge(cell, OrderSelectionController.MoveIcon);
                creature.AbandonTask();
                creature.Task = task;
            }
        };
    }

    private void SetActiveButton(ImageButton btn)
    {
        foreach (var button in _contextButtons)
        {
            button.Image.color = ColorConstants.GreyBase;
        }
        btn.Button.image.color = ColorConstants.BlueBase;
    }
}