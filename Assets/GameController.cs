using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public enum SelectionPreference
{
    CreatureOrStructure, Cell
}

public class GameController : MonoBehaviour
{
    public SelectionPreference SelectionPreference = SelectionPreference.CreatureOrStructure;
    internal List<StructureData> SelectedStructures = new List<StructureData>();
    internal List<CellData> SelectedCells = new List<CellData>();
    internal List<Creature> SelectedCreatures = new List<Creature>();
    public RectTransform selectSquareImage;
    private static GameController _instance;

    private TimeStep _oldTimeStep = TimeStep.Normal;

    private Vector3 _selectionEnd;
    private Vector3 _selectionStart;

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

    public void DeselectCell()
    {
        SelectedCells.Clear();
    }

    public void DeselectStructure()
    {
        foreach (var structure in SelectedStructures)
        {
            structure.LinkedGameObject.SpriteRenderer.color = StructureController.StructureColor;
        }
        SelectedStructures.Clear();
    }

    public void DeselectCreature()
    {
        foreach (var creature in SelectedCreatures)
        {
            creature.DisableHightlight();
        }
        CreatureInfoPanel.Instance.Hide();
        SelectedCreatures.Clear();
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
                TimeManager.Instance.Pause();
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

    private bool MouseOverUi()
    {
        var overUI = EventSystem.current.IsPointerOverGameObject() && EventSystem.current.currentSelectedGameObject != null;

        if (overUI)
        {
            selectSquareImage.gameObject.SetActive(false);
        }

        return overUI;
    }

    private void SelectCell()
    {
        if (SelectedCells.Count == 1)
        {
            var cell = SelectedCells.First();
            CellInfoPanel.Instance.Show(cell);
        }

        if (OrderSelectionController.Instance.CellClickOrder != null)
        {
            OrderSelectionController.Instance.CellClickOrder.Invoke(SelectedCells);
        }
    }

    private void SelectCreature()
    {
        DeselectCell();
        DeselectStructure();

        foreach (var creature in SelectedCreatures)
        {
            creature.EnableHighlight(Color.red);
        }

        if (SelectedCreatures.Count == 1)
        {
            CreatureInfoPanel.Instance.Show(SelectedCreatures.First());
        }
    }

    private void SelectStructure()
    {
        DeselectCell();
        DeselectCreature();

        foreach (var structure in SelectedStructures)
        {
            structure.LinkedGameObject.SpriteRenderer.color = Color.red;
        }

        if (SelectedStructures.Count == 1)
        {
            var structure = SelectedStructures[0];

            if (!structure.IsBluePrint && structure.Tasks.Count > 0)
            {
                CraftingScreen.Instance.Show(structure);
            }
        }
    }

    private void Start()
    {
        selectSquareImage.gameObject.SetActive(false);
    }

    private void Update()
    {
        HandleTimeControls();

        if (Input.GetMouseButton(1))
        {
            // right mouse deselect all
            DeselectCreature();
            DeselectCell();
            DeselectStructure();

            CraftingScreen.Instance.Hide();
            CreatureInfoPanel.Instance.Hide();
            CellInfoPanel.Instance.Hide();

            OrderSelectionController.Instance.DisableAndReset();
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                var rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                                         Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

                var hit = Physics2D.Raycast(rayPos, Vector2.zero, Mathf.Infinity);

                if (hit)
                {
                    _selectionStart = hit.point;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (MouseOverUi())
                {
                    return;
                }

                DeselectStructure();
                DeselectCreature();
                DeselectCell();
                selectSquareImage.gameObject.SetActive(false);

                var endPoint = Camera.main.ScreenToWorldPoint(_selectionEnd);

                var startX = Mathf.Clamp(Mathf.Min(_selectionStart.x, endPoint.x), 0, Constants.MapSize);
                var startY = Mathf.Clamp(Mathf.Min(_selectionStart.y, endPoint.y), 0, Constants.MapSize);
                var endX = Mathf.Clamp(Mathf.Max(_selectionStart.x, endPoint.x), 0, Constants.MapSize);
                var endY = Mathf.Clamp(Mathf.Max(_selectionStart.y, endPoint.y), 0, Constants.MapSize);

                if (startX == endX && startY == endY)
                {
                    var point = new Vector3(startX, endY);

                    var clickedCell = MapGrid.Instance.GetCellAtPoint(point);
                    if (clickedCell != null)
                    {
                        SelectedCells.Add(clickedCell);
                        if (clickedCell.Structure != null)
                        {
                            SelectedStructures.Add(clickedCell.Structure);
                        }
                    }

                    var clickedCreature = CreatureController.Instance.GetCreatureAtPoint(point);
                    if (clickedCreature != null)
                        SelectedCreatures.Add(clickedCreature);
                }
                else
                {
                    var pollStep = 1f;

                    for (var selX = startX; selX < endX; selX += pollStep)
                    {
                        for (var selY = startY; selY < endY; selY += pollStep)
                        {
                            var point = new Vector3(selX, selY);

                            var clickedCell = MapGrid.Instance.GetCellAtPoint(point);
                            if (clickedCell != null && !SelectedCells.Contains(clickedCell))
                            {
                                SelectedCells.Add(clickedCell);
                                if (clickedCell.Structure != null)
                                {
                                    SelectedStructures.Add(clickedCell.Structure);
                                }
                            }

                            var clickedCreature = CreatureController.Instance.GetCreatureAtPoint(point);
                            if (clickedCreature != null && !SelectedCreatures.Contains(clickedCreature))
                                SelectedCreatures.Add(clickedCreature);
                        }
                    }
                }

                switch (SelectionPreference)
                {
                    case SelectionPreference.Cell:
                        if (SelectedCells.Count > 0)
                        {
                            SelectCell();
                        }
                        break;

                    case SelectionPreference.CreatureOrStructure:
                        if (SelectedCreatures.Count > 0)
                        {
                            SelectCreature();
                        }
                        if (SelectedStructures.Count > 0)
                        {
                            SelectStructure();
                        }
                        break;
                }
            }

            if (Input.GetMouseButton(0))
            {
                if (MouseOverUi())
                {
                    return;
                }

                if (!selectSquareImage.gameObject.activeInHierarchy)
                {
                    selectSquareImage.gameObject.SetActive(true);
                }

                _selectionEnd = Input.mousePosition;

                var start = Camera.main.WorldToScreenPoint(_selectionStart);
                start.z = 0f;

                selectSquareImage.position = (start + _selectionEnd) / 2;

                var sizeX = Mathf.Abs(start.x - _selectionEnd.x);
                var sizeY = Mathf.Abs(start.y - _selectionEnd.y);

                selectSquareImage.sizeDelta = new Vector2(sizeX, sizeY);
            }
        }
    }
}