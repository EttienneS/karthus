using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    public bool BuildMode;
    internal List<Cell> SelectedCells = new List<Cell>();
    internal List<Creature> SelectedCreatures = new List<Creature>();

    private static GameController _instance;

    private TimeStep _oldTimeStep = TimeStep.Normal;

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
    }

    public void DeselectCreature()
    {
        foreach (var creature in SelectedCreatures)
        {
            creature.SpriteRenderer.color = Color.white;
        }
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

    public RectTransform selectSquareImage;

    private Vector3 _selectionStart;
    private Vector3 _selectionEnd;

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

            CreatureInfoPanel.Instance.Hide();
            CellInfoPanel.Instance.Hide();
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
                DeselectCreature();
                DeselectCell();

                //if (EventSystem.current.IsPointerOverGameObject())
                //{
                //    return;
                //}

                var clickedCreatures = new HashSet<Creature>();
                var clickedCells = new HashSet<Cell>();
                var endPoint = Camera.main.ScreenToWorldPoint(_selectionEnd);


                var startX = Mathf.Min(_selectionStart.x, endPoint.x);
                var startY = Mathf.Min(_selectionStart.y, endPoint.y);
                var endX = Mathf.Max(_selectionStart.x, endPoint.x);
                var endY = Mathf.Max(_selectionStart.y, endPoint.y);

                var pollStep = 1f;

                for (var selX = startX; selX < endX; selX += pollStep)
                {
                    for (var selY = startY; selY < endY; selY += pollStep)
                    {
                        var point = new Vector3(selX, selY);

                        var clickedCell = MapGrid.Instance.GetCellAtPoint(point);
                        if (clickedCell != null) clickedCells.Add(clickedCell);

                        var clickedCreature = CreatureController.Instance.GetCreatureAtPoint(point);
                        if (clickedCreature != null) clickedCreatures.Add(clickedCreature);
                    }

                }

                if (clickedCreatures.Count > 0)
                {
                    SelectedCreatures = clickedCreatures.ToList();
                    foreach (var creature in clickedCreatures)
                    {
                        creature.SpriteRenderer.color = new Color(Random.value, Random.value, Random.value);
                    }

                    //var clickedCreature = clickedCreatures.First();
                    //DeselectCreature();

                    //SelectedCreature = clickedCreature;

                    //CreatureInfoPanel.Instance.Show();


                }
                if (clickedCells.Count > 0)
                {
                    SelectedCells = clickedCells.ToList();

                    foreach (var cell in clickedCells)
                    {
                        cell.EnableBorder(Color.red);
                    }

                    //if (lastClickedCell == null || clickedCell != lastClickedCell)
                    //{
                    //    DeselectCell();

                    //    SelectedCell = clickedCell;
                    //    SelectedCell.EnableBorder(Color.red);

                    //    if (OrderSelectionController.Instance.CellClickOrder != null)
                    //    {
                    //        OrderSelectionController.Instance.CellClickOrder.Invoke(SelectedCell);
                    //    }

                    //    lastClickedCell = clickedCell;

                    //    CellInfoPanel.Instance.Show(SelectedCell);
                    //}
                }

                selectSquareImage.gameObject.SetActive(false);
            }

            if (Input.GetMouseButton(0))
            {
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