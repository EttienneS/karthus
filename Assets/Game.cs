using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;

public enum SelectionPreference
{
    CreatureOrStructure, Cell
}

public partial class Game : MonoBehaviour
{
    public SelectionPreference SelectionPreference = SelectionPreference.CreatureOrStructure;
    public RectTransform selectSquareImage;

    internal LineRenderer LineRenderer;
    internal List<Cell> SelectedCells = new List<Cell>();
    internal List<CreatureRenderer> SelectedCreatures = new List<CreatureRenderer>();
    internal List<Structure> SelectedStructures = new List<Structure>();

    private List<GameObject> _destroyCache = new List<GameObject>();
    private TimeStep _oldTimeStep = TimeStep.Normal;
    public Vector3 SelectionEnd;
    public Vector3 SelectionStart;

    public void AddItemToDestroy(GameObject gameObject)
    {
        lock (_destroyCache)
        {
            _destroyCache.Add(gameObject);
        }
    }

    public void AddLine(Cell start, Cell end)
    {
        LineRenderer.startColor = ColorConstants.InvalidColor;
        LineRenderer.endColor = ColorConstants.InvalidColor;

        LineRenderer.positionCount += 3;

        LineRenderer.SetPosition(LineRenderer.positionCount - 3, start.ToTopOfMapVector());
        LineRenderer.SetPosition(LineRenderer.positionCount - 2, end.ToTopOfMapVector());
        LineRenderer.SetPosition(LineRenderer.positionCount - 1, start.ToTopOfMapVector());

        LineRenderer.startWidth = 0.1f;
        LineRenderer.endWidth = 0.1f;
    }

    public void ClearLine()
    {
        LineRenderer.positionCount = 0;
    }

    public void DeselectAll()
    {
        DeselectCreature();
        DeselectCell();
        DeselectStructure(true);
    }

    public void DeselectCell()
    {
        SelectedCells.Clear();
    }

    public void DeselectCreature()
    {
        foreach (var creature in SelectedCreatures)
        {
            creature.DisableHightlight();
        }
        EntityInfoPanel.Hide();
        SelectedCreatures.Clear();
    }

    public void DeselectStructure(bool stopGhost)
    {
        if (stopGhost)
        {
            DisableMouseSprite();
        }

        ClearLine();
        foreach (var structure in SelectedStructures)
        {
            structure.HideOutline();
        }
        SelectedStructures.Clear();
    }

    public void DestroyItemsInCache()
    {
        try
        {
            lock (_destroyCache)
            {
                while (_destroyCache.Any())
                {
                    var item = _destroyCache[0];
                    _destroyCache.RemoveAt(0);
                    if (item != null)
                    {
                        Destroy(item);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Destroy failed: {ex}");
        }
    }

    private void HandleHotkeys()
    {
        if (Input.GetKeyDown("b"))
        {
            OrderSelectionController.BuildTypeClicked();
        }

        if (Input.GetKeyDown("n"))
        {
            OrderSelectionController.DesignateTypeClicked();
        }

        if (Input.GetKeyDown("e"))
        {
            RotateMouseRight?.Invoke();
        }

        if (Input.GetKeyDown("q"))
        {
            RotateMouseLeft?.Invoke();
        }

        if (Input.GetKeyDown("r"))
        {
            var point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var cell = Map.GetCellAtPoint(point);

            if (cell.Structure != null)
            {
                cell.Structure.RotateCW();
            }
        }
        if (Input.GetKeyDown("t"))
        {
            var point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var cell = Map.GetCellAtPoint(point);

            if (cell.Structure != null)
            {
                cell.Structure.RotateCCW();
            }
        }

        if (Input.GetKeyDown("f"))
        {
            var point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var cell = Map.GetCellAtPoint(point);

            VisualEffectController.SpawnLightEffect(cell, Color.blue, 2, 2, 4)
                                  .Fades();
            cell.FluidLevel += 0.5f;
            cell.UpdateTile();
        }

        if (Input.GetKeyDown("p"))
        {
            SaveManager.Save();
        }
        if (Input.GetKeyDown("["))
        {
            SaveManager.Load();
        }
    }

    private void HandleTimeControls()
    {
        if (Input.GetKeyDown("space"))
        {
            if (TimeManager.TimeStep == TimeStep.Paused)
            {
                TimeManager.TimeStep = _oldTimeStep;
            }
            else
            {
                _oldTimeStep = TimeManager.TimeStep;
                TimeManager.Pause();
            }
        }

        if (Input.GetKeyDown("1"))
        {
            TimeManager.TimeStep = TimeStep.Normal;
        }
        if (Input.GetKeyDown("2"))
        {
            TimeManager.TimeStep = TimeStep.Fast;
        }
        if (Input.GetKeyDown("3"))
        {
            TimeManager.TimeStep = TimeStep.Hyper;
        }
    }

    private void InitFactions()
    {
        foreach (var factionName in new[]
        {
            FactionConstants.Player,
            FactionConstants.Monster,
            FactionConstants.World
        })
        {
            var faction = new Faction
            {
                FactionName = factionName
            };
            FactionController.Factions.Add(factionName, faction);
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
        if (OrderSelectionController.CellClickOrder != null)
        {
            Debug.Log($"Clicked: {SelectedCells.Count}: {SelectedCells[0]}");
            OrderSelectionController.CellClickOrder.Invoke(SelectedCells);
            DeselectCell();
        }
    }

    private void SelectCreature()
    {
        DeselectCell();
        DeselectStructure(true);

        foreach (var creature in SelectedCreatures)
        {
            creature.EnableHighlight(ColorConstants.InvalidColor);
        }

        EntityInfoPanel.Show(SelectedCreatures.Select(c => c.Data).ToList());
    }

    private void SelectStructure()
    {
        foreach (var structure in SelectedStructures)
        {
            structure.ShowOutline();
            var id = structure.Id;
            foreach (var boundCell in Map.Cells.Where(c => c.Binding == structure))
            {
                AddLine(structure.Cell, boundCell);
            }
        }

        EntityInfoPanel.Show(SelectedStructures.ToList());
    }

    private void Start()
    {
        var sw = new Stopwatch();
        sw.Start();
        Debug.Log("Start mapgen");

        LineRenderer = GetComponent<LineRenderer>();

        selectSquareImage.gameObject.SetActive(false);
        MouseSpriteRenderer.gameObject.SetActive(false);

        InitFactions();

        MapGenerator = new MapGenerator();
        MapGenerator.Make();

        Debug.Log($"Map gen complete in: {sw.Elapsed}");
    }

    public float MaxTimeToClick { get; set; } = 0.60f;
    public float MinTimeToClick { get; set; } = 0.05f;

    private float _minCurrentTime;
    private float _maxCurrentTime;

    public bool DoubleClick()
    {
        if (Time.time >= _minCurrentTime && Time.time <= _maxCurrentTime)
        {
            _minCurrentTime = 0;
            _maxCurrentTime = 0;
            return true;
        }
        _minCurrentTime = Time.time + MinTimeToClick; _maxCurrentTime = Time.time + MaxTimeToClick;
        return false;
    }

    private void Update()
    {
        var mousePosition = Input.mousePosition;

        HandleHotkeys();
        HandleTimeControls();
        MoveMouseSprite(mousePosition);

        if (Input.GetMouseButton(1))
        {
            // right mouse deselect all
            DeselectAll();

            EntityInfoPanel.Hide();
            DisableMouseSprite();
            OrderSelectionController.DisableAndReset();
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (MouseOverUi())
                {
                    return;
                }

                var point = Camera.main.ScreenToWorldPoint(mousePosition);
                //if (DoubleClick())
                //{
                //    var cell = Map.GetCellAtPoint(point);

                //    if (cell.Creatures.Count == 0)
                //    {
                //        SelectedCreatures.AddRange(cell.Creatures
                //                                       .FirstOrDefault()?.GetFaction()
                //                                       .Creatures.Select(c => c.CreatureRenderer));
                //    }
                //}
                //else
                //{
                SelectionStart = point;
                //}
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (MouseOverUi())
                {
                    return;
                }

                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)
                    && OrderSelectionController.CellClickOrder == null)
                {
                    DeselectAll();
                }

                selectSquareImage.gameObject.SetActive(false);

                var endPoint = Camera.main.ScreenToWorldPoint(SelectionEnd);

                var startX = Mathf.Clamp(Mathf.Min(SelectionStart.x, endPoint.x), 0, Map.Width);
                var startY = Mathf.Clamp(Mathf.Min(SelectionStart.y, endPoint.y), 0, Map.Height);
                var endX = Mathf.Clamp(Mathf.Max(SelectionStart.x, endPoint.x), 0, Map.Width);
                var endY = Mathf.Clamp(Mathf.Max(SelectionStart.y, endPoint.y), 0, Map.Height);

                if (startX == endX && startY == endY)
                {
                    var point = new Vector3(startX, endY);

                    var clickedCell = Map.GetCellAtPoint(point);
                    if (clickedCell != null)
                    {
                        SelectedCells.Add(clickedCell);
                        if (clickedCell.Structure != null)
                        {
                            SelectedStructures.Add(clickedCell.Structure);
                        }
                    }

                    var clickedCreature = CreatureController.GetCreatureAtPoint(point);
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

                            var clickedCell = Map.GetCellAtPoint(point);
                            if (clickedCell != null && !SelectedCells.Contains(clickedCell))
                            {
                                SelectedCells.Add(clickedCell);
                                if (clickedCell.Structure != null)
                                {
                                    SelectedStructures.Add(clickedCell.Structure);
                                }
                            }

                            var clickedCreature = CreatureController.GetCreatureAtPoint(point);
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

                SelectionEnd = mousePosition;

                var start = Camera.main.WorldToScreenPoint(SelectionStart);
                start.z = 0f;

                selectSquareImage.position = (start + SelectionEnd) / 2;

                var sizeX = Mathf.Abs(start.x - SelectionEnd.x);
                var sizeY = Mathf.Abs(start.y - SelectionEnd.y);

                selectSquareImage.sizeDelta = new Vector2(sizeX, sizeY);
            }
        }
        DestroyItemsInCache();
    }
}