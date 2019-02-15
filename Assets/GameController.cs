using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    public bool BuildMode;
    public RectTransform selectSquareImage;
    internal List<Cell> SelectedCells = new List<Cell>();
    internal List<Creature> SelectedCreatures = new List<Creature>();

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
        foreach (var cell in SelectedCells)
        {
            cell.DisableBorder();
        }

        SelectedCells.Clear();
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

    private void Start()
    {
        selectSquareImage.gameObject.SetActive(false);
    }

    public SelectionPreference SelectionPreference = SelectionPreference.CreatureOnly;

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

                DeselectCreature();
                DeselectCell();

                var endPoint = Camera.main.ScreenToWorldPoint(_selectionEnd);

                var startX = Mathf.Min(_selectionStart.x, endPoint.x);
                var startY = Mathf.Min(_selectionStart.y, endPoint.y);
                var endX = Mathf.Max(_selectionStart.x, endPoint.x);
                var endY = Mathf.Max(_selectionStart.y, endPoint.y);

                if (startX == endX && startY == endY)
                {
                    var point = new Vector3(startX, endY);

                    var clickedCell = MapGrid.Instance.GetCellAtPoint(point);
                    if (clickedCell != null)
                        SelectedCells.Add(clickedCell);

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
                                SelectedCells.Add(clickedCell);

                            var clickedCreature = CreatureController.Instance.GetCreatureAtPoint(point);
                            if (clickedCreature != null && !SelectedCreatures.Contains(clickedCreature))
                                SelectedCreatures.Add(clickedCreature);
                        }
                    }
                }

                switch (SelectionPreference)
                {
                    case SelectionPreference.CellOnly:
                        if (SelectedCells.Count > 0)
                        {
                            SelectCell();
                        }
                        break;
                    case SelectionPreference.CreatureOnly:
                        if (SelectedCreatures.Count > 0)
                        {
                            SelectCreature();
                        }
                        break;
                }

                selectSquareImage.gameObject.SetActive(false);
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

    private void SelectCell()
    {
        foreach (var cell in SelectedCells)
        {
            cell.EnableBorder(Color.red);
        }

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
        foreach (var creature in SelectedCreatures)
        {
            creature.EnableHighlight(Color.red);
        }

        if (SelectedCreatures.Count == 1)
        {
            CreatureInfoPanel.Instance.Show(SelectedCreatures.First());
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
}

public enum SelectionPreference
{
    CreatureOnly, CellOnly
}