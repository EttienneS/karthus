using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    public bool BuildMode;
    public Cell SelectedCell;
    public List<Creature> SelectedCreatures;
    private static GameController _instance;

    private TimeStep _oldTimeStep = TimeStep.Normal;
    private Cell lastClickedCell;

    public static GameController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("GameController").GetComponent<GameController>();
            }

            return _instance;
        }
    }

    public Creature SelectedCreature { get; set; }

    public void DeselectCell()
    {
        if (SelectedCell != null)
        {
            SelectedCell.DisableBorder();
            SelectedCell = null;
        }
    }

    public void DeselectCreature()
    {
        if (SelectedCreature != null)
        {
            SelectedCreature.Outline.outlineSize = 0;
            SelectedCreature = null;
        }
    }

    private static void DestroyAndRecreateMap()
    {
        foreach (var cell in MapGrid.Instance.Map)
        {
            Destroy(cell.gameObject);
        }

        foreach (var creature in CreatureController.Instance.Creatures)
        {
            Destroy(creature.gameObject);
        }
        CreatureController.Instance.Creatures.Clear();

        MapEditor.Instance.Generating = false;
    }

    private void HandleTimeControls()
    {
        if (Input.GetKeyDown("space"))
        {
            if (TimeManager.Instance.TimeStep == TimeStep.Paused)
            {
                TimeManager.Instance.TimeStep = _oldTimeStep;
            }
            else
            {
                _oldTimeStep = TimeManager.Instance.TimeStep;
                TimeManager.Instance.TimeStep = TimeStep.Paused;
            }
        }

        if (Input.GetKeyDown("1"))
        {
            TimeManager.Instance.TimeStep = TimeStep.Slow;
        }
        if (Input.GetKeyDown("2"))
        {
            TimeManager.Instance.TimeStep = TimeStep.Normal;
        }
        if (Input.GetKeyDown("3"))
        {
            TimeManager.Instance.TimeStep = TimeStep.Fast;
        }
        if (Input.GetKeyDown("4"))
        {
            TimeManager.Instance.TimeStep = TimeStep.Hyper;
        }
    }

    private void Update()
    {
        HandleTimeControls();

        if (Input.GetMouseButton(1))
        {
            // right mouse deselect all
            DeselectCreature();
            DeselectCell();

            CreatureInfoPanel.Instance.Hide();
            CellInfoPanel.Instance.Hide();
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                var rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                                         Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

                var hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);
                if (hit)
                {
                    var clickedCell = MapGrid.Instance.GetCellAtPoint(hit.point);
                    var clickedCreature = CreatureController.Instance.GetCreatureAtPoint(hit.point);

                    if (clickedCreature != null)
                    {
                        DeselectCreature();

                        SelectedCreature = clickedCreature;
                        SelectedCreature.Outline.outlineSize = 1;

                        CreatureInfoPanel.Instance.Show();
                    }
                    else
                    {
                        if (lastClickedCell == null || clickedCell != lastClickedCell)
                        {
                            DeselectCell();

                            SelectedCell = clickedCell;
                            SelectedCell.EnableBorder(Color.red);

                            if (OrderSelectionController.Instance.CellClickOrder != null)
                            {
                                OrderSelectionController.Instance.CellClickOrder.Invoke(SelectedCell);
                            }

                            lastClickedCell = clickedCell;

                            CellInfoPanel.Instance.Show(SelectedCell);
                        }
                    }
                }
            }
        }
    }
}