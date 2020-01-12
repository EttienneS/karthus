using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EntityInfoPanel : MonoBehaviour
{
    public GameObject ButtonPanel;
    public Text CreatureName;
    public List<IEntity> CurrentEntities;
    public Toggle FirstPanelToggle;
    public Text HealthText;
    public ImageButton ImageButtonPrefab;
    public Text Log;
    public ManaPanel ManaPanel;
    public Text PropertiesPanel;
    public GameObject TabPanel;
    private List<ImageButton> _contextButtons = new List<ImageButton>();

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

    public void Show(IEnumerable<IEntity> entities)
    {
        gameObject.SetActive(true);

        // switch to overview panel
        FirstPanelToggle.isOn = true;

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

        if (entities.First() is Creature creature)
        {
            // creatures
            var creatures = entities.OfType<Creature>();
            AddMoveButton(creatures);
            AddAttackButton(creatures);

            ManaPanel.SetPool(creature.ManaPool);
        }
        else if (entities.First() is Structure)
        {
            // structures
            AddRemoveStructureButton(entities.OfType<Structure>());
            AddCycleModeButton(entities.OfType<Structure>());
            AddRotateModeButtons(entities.OfType<Structure>());
        }
        else
        {
            // items
        }
    }

    public void Update()
    {
        if (CurrentEntities != null)
        {
            PropertiesPanel.text = string.Empty;
            TabPanel.SetActive(false);

            if (CurrentEntities.Count == 1)
            {
                var currentEntity = CurrentEntities[0];

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
                    TabPanel.SetActive(true);

                    foreach (var line in creature.LogHistory)
                    {
                        Log.text += $"{line}\n";
                    }

                    var rt = Log.GetComponent(typeof(RectTransform)) as RectTransform;
                    rt.sizeDelta = new Vector2(395, creature.LogHistory.Count * 20);

                    PropertiesPanel.text += "\nSkills: \n\n";

                    foreach (var skill in creature.Skills)
                    {
                        PropertiesPanel.text += $"\t{skill}\n";
                    }
                    PropertiesPanel.text += "\n";
                    PropertiesPanel.text += $"\nLocation:\t{creature.X:F1}:{creature.Y:F1}\n\n";

                    

                    LogHealth(creature);
                    LogTask(creature);
                }
                else
                {
                    PropertiesPanel.text += $"\nLocation:\t{currentEntity.Cell}\n\n";

                    if (currentEntity is Structure structure)
                    {
                        PropertiesPanel.text += $"\nMana:\t{structure.Cost.Mana.GetString(1)}\n\n";

                        PropertiesPanel.text += $"Rotation:\t {structure.Rotation}\n";

                        if (structure.IsBluePrint)
                        {
                            PropertiesPanel.text += "\n*Blueprint, waiting for construction...*\n";
                        }
                        else
                        {
                            if (structure.InUseByAnyone)
                            {
                                PropertiesPanel.text += $"In use by:\t{structure.InUseBy.Name}\n";
                            }

                            if (structure is Container container)
                            {
                                PropertiesPanel.text += $"\nContainer:\n";

                                if (container.Empty)
                                {
                                    PropertiesPanel.text += $"Contains:\tNothing\n";
                                }
                                else
                                {
                                    PropertiesPanel.text += $"Contains:\t{container.ItemType}\n";
                                }

                                PropertiesPanel.text += $"Capacity:\t{container.Count}/{container.Capacity}\n";
                                PropertiesPanel.text += $"Filter:\t{container.Filter}\n";
                            }
                        }

                        foreach (var interaction in structure.AutoInteractions)
                        {
                            PropertiesPanel.text += $"{interaction}: {interaction.Elapsed}/{interaction.ActivationTime}\n";
                        }
                    }
                    else if (currentEntity is Item item)
                    {
                        PropertiesPanel.text += $"\nMana:\t{item.Cost.Mana.GetString(item.Amount)}\n\n";

                        PropertiesPanel.text += $"Amount:\t {item.Amount}\n";
                        if (item.InUseByAnyone)
                        {
                            PropertiesPanel.text += $"In use by:\t{item.InUseBy.Name}\n";
                        }
                    }
                }
            }
            else
            {
                CreatureName.text = $"{CurrentEntities.Count} entities";

                //var entityGroups = CurrentEntities.GroupBy(e => e.Name).Select(e => new
                //{
                //    Text = e.Key,
                //    Count = e.Count(),
                //    Green = e.Sum(g => g.ManaValue.GetTotal(ManaColor.Green)),
                //    Red = e.Sum(r => r.ManaValue.GetTotal(ManaColor.Red)),
                //    Blue = e.Sum(u => u.ManaValue.GetTotal(ManaColor.Blue)),
                //    Black = e.Sum(b => b.ManaValue.GetTotal(ManaColor.Black)),
                //    White = e.Sum(w => w.ManaValue.GetTotal(ManaColor.White))
                //});

                //foreach (var group in entityGroups)
                //{
                //    PropertiesPanel.text += $"- {group.Text} x{group.Count} (R:{group.Red}, G:{group.Green}, U:{group.Blue}, B:{group.Black}, W:{group.White})\n";
                //}
            }
        }
    }

    private void AddAttackButton(IEnumerable<Creature> creatures)
    {
        var btn = AddButton(OrderSelectionController.AttackText, OrderSelectionController.AttackIcon);
        btn.SetOnClick(() => AttackClicked(creatures, btn));
    }

    private void AddCycleModeButton(IEnumerable<Structure> structures)
    {
        var prime = structures.First();

        if (!structures.All(s => s.Name == prime.Name && s.AutoInteractions.Count > 0))
        {
            return;
        }

        foreach (var interaction in prime.AutoInteractions)
        {
            if (string.IsNullOrEmpty(interaction.DisplayName))
            {
                continue;
            }

            var btn = AddButton($"{interaction.DisplayName} - {(interaction.Disabled ? "Off" : "On")}", interaction.Disabled ? "check_mark_t" : "quest_complete_t");
            btn.SetOnClick(() =>
            {
                SetActiveButton(btn);
                interaction.Disabled = !interaction.Disabled;

                foreach (var structure in structures)
                {
                    structure.AutoInteractions[prime.AutoInteractions.IndexOf(interaction)].Disabled = interaction.Disabled;
                }

                btn.Text.text = $"{interaction.DisplayName} - {(interaction.Disabled ? "Off" : "On")}";
                btn.Image.sprite = Game.SpriteStore.GetSprite(interaction.Disabled ? "check_mark_t" : "quest_complete_t");
            });
        }
    }

   
    private void AddMoveButton(IEnumerable<Creature> creatures)
    {
        var btn = AddButton(OrderSelectionController.MoveText, OrderSelectionController.MoveIcon);
        btn.SetOnClick(() => MoveClicked(creatures, btn));
    }

    private void AddRemoveStructureButton(IEnumerable<Structure> structures)
    {
        var btn = AddButton(OrderSelectionController.DefaultRemoveText, OrderSelectionController.DefaultRemoveIcon);
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
                    if (Game.FactionController.PlayerFaction.AvailableTasks.OfType<RemoveStructure>().Any(t => t.StructureToRemove == structure))
                    {
                        Debug.Log("Structure already flagged to remove");
                    }
                    else
                    {
                        Game.FactionController.PlayerFaction
                                         .AddTask(new RemoveStructure(structure))
                                         .AddCellBadge(structure.Cell,
                                                       OrderSelectionController.DefaultRemoveIcon);
                    }
                }
            }
        });
    }

    private void AddRotateModeButtons(IEnumerable<Structure> structures)
    {
        var btnCW = AddButton($"Rotate Clockwise", "rotate_cw");
        btnCW.SetOnClick(() =>
        {
            SetActiveButton(btnCW);

            foreach (var structure in structures)
            {
                structure.RotateCW();
            }
        });

        var btnCCW = AddButton($"Rotate Counter-Clockwise", "rotate_ccw");
        btnCCW.SetOnClick(() =>
        {
            SetActiveButton(btnCCW);

            foreach (var structure in structures)
            {
                structure.RotateCCW();
            }
        });
    }

    private void AttackClicked(IEnumerable<Creature> creatures, ImageButton btn)
    {
        SetActiveButton(btn);

        Game.Controller.SelectionPreference = SelectionPreference.Cell;
        Game.Controller.SetMouseSprite(OrderSelectionController.AttackIcon,
                                        (cell) => cell.GetEnemyCreaturesOf(FactionConstants.Player).Any());

        Game.OrderSelectionController.CellClickOrder = cells =>
        {
            foreach (var creature in creatures)
            {
                foreach (var cell in cells)
                {
                    foreach (var enemy in cell.GetEnemyCreaturesOf(creature.FactionName))
                    {
                        creature.CancelTask();
                        creature.Combatants.Add(enemy);
                        break;
                    }
                }
            }
        };
    }

    private void LogHealth(Creature creature)
    {
        HealthText.text = "\nBody: \n\n";

        foreach (var limb in creature.Limbs)
        {
            HealthText.text += $"\t{limb}\n";
        }
        HealthText.text += "\n";

      
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

    private void MoveClicked(IEnumerable<Creature> creatures, ImageButton btn)
    {
        SetActiveButton(btn);
        Game.Controller.SelectionPreference = SelectionPreference.Cell;
        Game.Controller.SetMouseSprite(OrderSelectionController.MoveIcon, (cell) => cell.TravelCost > 0);

        Game.OrderSelectionController.CellClickOrder = cells =>
        {
            foreach (var creature in creatures)
            {
                var cell = cells[0];

                var faction = creature.GetFaction();
                var task = new Move(cell);
                task.AddCellBadge(cell, OrderSelectionController.MoveIcon);
                creature.CancelTask();
                creature.Task = task;
            }
        };
    }

    private void SetActiveButton(ImageButton btn)
    {
        foreach (var button in _contextButtons)
        {
            button.Image.color = Color.white;
        }
        btn.Button.image.color = Color.red;
    }
}