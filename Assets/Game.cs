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
    private Vector3 _selectionEnd;
    private Vector3 _selectionStart;

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
        CreatureInfoPanel.Hide();
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
            var cell = structure.Cell;
            //structure.LinkedGameObject.SpriteRenderer.color = cell.Bound ? ColorConstants.BaseColor :
            //                                                               ColorConstants.UnboundColor;
        }
        SelectedStructures.Clear();
    }

    private void DestroyItemsInCache()
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

        //if (Input.GetKeyDown("u"))
        //{
        //    var cell = MapGrid.GetRandomCell();
        //    CameraController.MoveToCell(cell);

        //    var texture = MapGrid.ChangeCell(cell, CellType.Abyss);

        //    MapGrid.UpdateSprite(texture);
        //}

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
            var factionBody = StructureController.GetStructure(FactionConstants.StructureName, null);
            var faction = new GameObject(factionName, typeof(Faction)).GetComponent<Faction>();
            faction.transform.SetParent(transform);

            faction.FactionName = factionName;
            faction.Core = factionBody;

            faction.AddStructure(factionBody);

            if (factionName != FactionConstants.Player)
            {
                factionBody.Spell = null;
            }

            faction.transform.position = new Vector2(-100, -100);
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
        if (SelectedCells.Count == 1)
        {
            var cell = SelectedCells.First();
            CellInfoPanel.Show(cell);
        }

        if (OrderSelectionController.CellClickOrder != null)
        {
            Debug.Log($"Clicked: {SelectedCells.Count}: {SelectedCells[0]}");
            OrderSelectionController.CellClickOrder.Invoke(SelectedCells);
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

        if (SelectedCreatures.Count == 1)
        {
            CreatureInfoPanel.Show(SelectedCreatures.First());
        }
    }

    private void SelectStructure()
    {
        DeselectCell();
        DeselectCreature();

        foreach (var structure in SelectedStructures)
        {
            var id = structure.Id;
            if (MapGrid.CellBinding.ContainsKey(id))
            {
                foreach (var boundCell in MapGrid.CellBinding[id])
                {
                    AddLine(structure.Cell, boundCell);
                }
            }
        }
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

    private void Update()
    {
        var mousePosition = Input.mousePosition;

        HandleHotkeys();
        HandleTimeControls();
        MoveMouseSprite(mousePosition);

        if (Input.GetMouseButton(1))
        {
            // right mouse deselect all
            DeselectCreature();
            DeselectCell();
            DeselectStructure(true);

            //CraftingScreen.Hide();
            CreatureInfoPanel.Hide();
            CellInfoPanel.Hide();

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

                _selectionStart = Camera.main.ScreenToWorldPoint(mousePosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (MouseOverUi())
                {
                    return;
                }

                DeselectStructure(false);
                DeselectCreature();
                DeselectCell();
                selectSquareImage.gameObject.SetActive(false);

                var endPoint = Camera.main.ScreenToWorldPoint(_selectionEnd);

                var startX = Mathf.Clamp(Mathf.Min(_selectionStart.x, endPoint.x), 0, MapGrid.Width);
                var startY = Mathf.Clamp(Mathf.Min(_selectionStart.y, endPoint.y), 0, MapGrid.Height);
                var endX = Mathf.Clamp(Mathf.Max(_selectionStart.x, endPoint.x), 0, MapGrid.Width);
                var endY = Mathf.Clamp(Mathf.Max(_selectionStart.y, endPoint.y), 0, MapGrid.Height);

                if (startX == endX && startY == endY)
                {
                    var point = new Vector3(startX, endY);

                    var clickedCell = MapGrid.GetCellAtPoint(point);
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

                            var clickedCell = MapGrid.GetCellAtPoint(point);
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

                _selectionEnd = mousePosition;

                var start = Camera.main.WorldToScreenPoint(_selectionStart);
                start.z = 0f;

                selectSquareImage.position = (start + _selectionEnd) / 2;

                var sizeX = Mathf.Abs(start.x - _selectionEnd.x);
                var sizeY = Mathf.Abs(start.y - _selectionEnd.y);

                selectSquareImage.sizeDelta = new Vector2(sizeX, sizeY);
            }
        }
        DestroyItemsInCache();
    }
}