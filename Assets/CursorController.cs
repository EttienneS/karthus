﻿using Assets.Helpers;
using Assets.Structures;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets
{
    public class CursorController : MonoBehaviour
    {
        public Light CursorLight;
        public SpriteRenderer MouseSpriteRenderer;
        public RectTransform SelectSquareImage;
        public ValidateMouseDelegate Validate;

        private readonly Dictionary<Cell, MeshRenderer> _draggedRenderers = new Dictionary<Cell, MeshRenderer>();
        private Sprite _currentSprite;
        private SelectionPreference _lastSelection = SelectionPreference.Creature;
        private string _meshName;
        private MeshRenderer _meshRenderer;
        private RotateDelegate _rotateLeft;
        private RotateDelegate _rotateRight;
        private SelectionPreference _selectionPreference;
        private Vector3 SelectionEndScreen;
        private Vector3 SelectionStartWorld;

        public delegate void RotateDelegate();

        public delegate bool ValidateMouseDelegate(Cell cell);

        public void ResetSelection()
        {
            SelectionStartWorld = Vector3.zero;
            SetSelectionPreference(SelectionPreference.Anything);
        }

        public void RotateLeft()
        {
            _rotateLeft?.Invoke();
        }

        public void RotateRight()
        {
            _rotateRight?.Invoke();
        }

        public void SetMesh(string name, ValidateMouseDelegate validationFunction)
        {
            Clear();
            _meshName = name;

            var structure = Game.Instance.StructureController.InstantiateNewStructureMeshRenderer(name, transform);
            structure.SetAllMaterial(Game.Instance.FileController.BlueprintMaterial);

            _meshRenderer = structure;
            Validate = validationFunction;
        }

        public void SetMultiSprite(Sprite sprite, ValidateMouseDelegate validationFunction)
        {
            SetSprite(sprite, validationFunction);

            var offsetX = ((sprite.texture.width / Map.PixelsPerCell) - 1) / 2f;
            var offsetZ = ((sprite.texture.height / Map.PixelsPerCell) - 1) / 2f;

            MouseSpriteRenderer.transform.localPosition = new Vector3(offsetX, 0.1f, offsetZ);
        }

        public void SetSelectionPreference(SelectionPreference preference)
        {
            _selectionPreference = preference;
        }

        public void SetSprite(Sprite sprite, ValidateMouseDelegate validationFunction)
        {
            MouseSpriteRenderer.sprite = sprite;
            Validate = validationFunction;

            _currentSprite = sprite;

            MouseSpriteRenderer.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        }

        public void ShowConstructGhost(Construct construct)
        {
            _rotateLeft = () =>
            {
                construct.RotateRight();
                Game.Instance.Cursor.SetMultiSprite(construct.GetSprite(), (cell) => construct.ValidateStartPos(cell));
            };
            _rotateRight = () =>
            {
                construct.RotateLeft();
                Game.Instance.Cursor.SetMultiSprite(construct.GetSprite(), (cell) => construct.ValidateStartPos(cell));
            };

            Validate = (cell) => construct.ValidateStartPos(cell);
            Game.Instance.Cursor.SetMultiSprite(construct.GetSprite(), (cell) => construct.ValidateStartPos(cell));
        }

        public void Start()
        {
            HideSelectionRectangle();
        }

        public void Update()
        {
            CursorLight.enabled = Game.Instance.TimeManager.Data.Hour > 17 || Game.Instance.TimeManager.Data.Hour < 5;

            var pos = GetWorldMousePosition();
            if (pos != null)
            {
                var startCell = Game.Instance.Map.GetCellAtCoordinate(pos.Value);
                if (startCell == null)
                {
                    return;
                }
                transform.position = new Vector3(startCell.Vector.x,
                                                 Game.Instance.MapData.StructureLevel,
                                                 startCell.Vector.z) + new Vector3(0, 0.25f, 0);

                ValidateCursor(startCell);
            }

            if (!string.IsNullOrEmpty(_meshName))
            {
                var selectedCells = GetSelectedCells();

                if (selectedCells == null || selectedCells.Count == 0)
                {
                    return;
                }

                foreach (var cell in selectedCells)
                {
                    MeshRenderer cellRenderer;
                    if (!_draggedRenderers.ContainsKey(cell))
                    {
                        cellRenderer = Game.Instance.StructureController.InstantiateNewStructureMeshRenderer(_meshName, Game.Instance.Map.transform);
                        cellRenderer.transform.position = new Vector3(cell.Vector.x, Game.Instance.MapData.StructureLevel, cell.Vector.z);
                        _draggedRenderers.Add(cell, cellRenderer);
                    }
                    else
                    {
                        cellRenderer = _draggedRenderers[cell];
                    }

                    if (Validate?.Invoke(cell) == false)
                    {
                        cellRenderer.SetAllMaterial(Game.Instance.FileController.InvalidBlueprintMaterial);
                    }
                    else
                    {
                        cellRenderer.SetAllMaterial(Game.Instance.FileController.BlueprintMaterial);
                    }
                }

                foreach (var cell in _draggedRenderers.Keys.ToList())
                {
                    if (!selectedCells.Contains(cell))
                    {
                        Destroy(_draggedRenderers[cell].gameObject);
                        _draggedRenderers.Remove(cell);
                    }
                }
            }

            HandleMouseInput();
        }

        private void Clear()
        {
            MouseSpriteRenderer.sprite = null;
            MouseSpriteRenderer.size = Vector2.one;

            foreach (var meshRenderer in _draggedRenderers.Values)
            {
                Destroy(meshRenderer.gameObject);
            }
            _draggedRenderers.Clear();

            if (_meshRenderer != null)
            {
                Destroy(_meshRenderer.gameObject);
                _meshRenderer = null;
            }

            _meshName = string.Empty;

            if (_currentSprite != null)
            {
                MouseSpriteRenderer.sprite = null;
                _currentSprite = null;
            }
        }

        private void DeselectAll()
        {
            Clear();
            DeselectCreature();
            DeselectStructure();
            DeselectItem();
            DeselectZone();
        }

        private void DeselectCreature()
        {
            foreach (var creature in Game.Instance.IdService.CreatureIdLookup.Values.Select(v => v.CreatureRenderer))
            {
                creature.DisableHightlight();
            }

            Game.Instance.DestroyCreaturePanel();
            Game.Instance.DestroyToolTip();
        }

        private void DeselectItem()
        {
            foreach (var item in Game.Instance.IdService.ItemLookup.Values)
            {
                item.HideOutline();
            }
            Game.Instance.DestroyItemInfoPanel();
        }

        private void DeselectStructure()
        {
            foreach (var structure in Game.Instance.IdService.StructureIdLookup.Values)
            {
                structure.HideOutline();
            }
            Game.Instance.DestroyStructureInfoPanel();
        }

        private void DeselectZone()
        {
            Game.Instance.DestroyZonePanel();
        }

        private List<CreatureRenderer> FindCreaturesInCells(List<Cell> cells)
        {
            var creatures = new List<CreatureRenderer>();
            foreach (var cell in cells)
            {
                creatures.AddRange(cell.Creatures.Select(c => c.CreatureRenderer));
            }

            return creatures;
        }

        private List<Item> FindItemsInCells(List<Cell> cells)
        {
            var items = new List<Item>();
            foreach (var cell in cells)
            {
                items.AddRange(cell.Items);
            }

            return items;
        }

        private List<Structure> FindStructuresInCells(List<Cell> cells)
        {
            var structures = new List<Structure>();
            foreach (var cell in cells)
            {
                if (cell.Structure != null)
                {
                    structures.Add(cell.Structure);
                }
            }

            return structures;
        }

        private ZoneBase FindZoneInCells(List<Cell> cells)
        {
            foreach (var cell in cells)
            {
                var zone = Game.Instance.ZoneController.GetZoneForCell(cell);
                if (zone != null)
                {
                    return zone;
                }
            }

            return null;
        }

        private List<Cell> GetSelectedCells()
        {
            if (SelectionStartWorld == Vector3.zero)
            {
                return new List<Cell>();
            }
            var worldStartPoint = SelectionStartWorld;
            var worldEndPoint = GetWorldMousePosition().Value;

            var cells = new List<Cell>();

            var startX = Mathf.Clamp(Mathf.Min(worldStartPoint.x, worldEndPoint.x), Game.Instance.Map.MinX, Game.Instance.Map.MaxX);
            var endX = Mathf.Clamp(Mathf.Max(worldStartPoint.x, worldEndPoint.x), Game.Instance.Map.MinX, Game.Instance.Map.MaxX);

            var startZ = Mathf.Clamp(Mathf.Min(worldStartPoint.z, worldEndPoint.z), Game.Instance.Map.MinZ, Game.Instance.Map.MaxZ);
            var endZ = Mathf.Clamp(Mathf.Max(worldStartPoint.z, worldEndPoint.z), Game.Instance.Map.MinX, Game.Instance.Map.MaxZ);

            // not currently used
            var startY = Mathf.Min(worldStartPoint.y, worldEndPoint.y);
            var endY = Mathf.Max(worldStartPoint.y, worldEndPoint.y);

            if (startX == endX && startZ == endZ)
            {
                var point = new Vector3(startX, startY, startZ);

                var clickedCell = Game.Instance.Map.GetCellAtPoint(point);
                if (clickedCell != null)
                {
                    cells.Add(clickedCell);
                }
            }
            else
            {
                var pollStep = 1f;

                for (var selX = startX; selX < endX; selX += pollStep)
                {
                    for (var selZ = startZ; selZ < endZ; selZ += pollStep)
                    {
                        var point = new Vector3(selX, startY, selZ);

                        cells.Add(Game.Instance.Map.GetCellAtPoint(point));
                    }
                }
            }

            return cells.Distinct().ToList();
        }

        private bool GetSelection(SelectionPreference selectionPreference, List<Cell> cells)
        {
            SelectionStartWorld = Vector3.zero;

            switch (selectionPreference)
            {
                case SelectionPreference.Anything:

                    for (int i = 0; i < 5; i++)
                    {
                        switch (_lastSelection)
                        {
                            case SelectionPreference.Creature:
                                _lastSelection = SelectionPreference.Structure;
                                break;

                            case SelectionPreference.Structure:
                                _lastSelection = SelectionPreference.Item;
                                break;

                            case SelectionPreference.Item:
                                _lastSelection = SelectionPreference.Zone;
                                break;

                            case SelectionPreference.Zone:
                                _lastSelection = SelectionPreference.Creature;
                                break;
                        }
                        if (GetSelection(_lastSelection, cells))
                        {
                            break;
                        }
                    }
                    break;

                case SelectionPreference.Cell:
                    if (cells.Count > 0)
                    {
                        if (Game.Instance.OrderSelectionController.CellClickOrder != null)
                        {
                            Game.Instance.OrderSelectionController.CellClickOrder.Invoke(cells);
                        }
                        return true;
                    }
                    break;

                case SelectionPreference.Item:
                    return SelectItems(FindItemsInCells(cells));

                case SelectionPreference.Structure:
                    return SelectStructures(FindStructuresInCells(cells));

                case SelectionPreference.Creature:
                    return SelectCreatures(FindCreaturesInCells(cells));

                case SelectionPreference.Zone:
                    return SelectZone(FindZoneInCells(cells));
            }
            return false;
        }

        private Vector3? GetWorldMousePosition()
        {
            var inputRay = Game.Instance.CameraController.Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(inputRay, out RaycastHit hit))
            {
                return hit.point;
            }
            return null;
        }

        private void HandleMouseInput()
        {
            if (MouseOverUi())
            {
                return;
            }

            if (InputHelper.RightMouseClciked())
            {
                // right mouse deselect all
                DeselectAll();
                Game.Instance.OrderSelectionController.DisableAndReset();
            }
            else
            {
                var worldMousePosition = GetWorldMousePosition();

                if (worldMousePosition != null)
                {
                    if (InputHelper.LeftMouseButtonStartClick())
                    {
                        SelectionStartWorld = GetWorldMousePosition().Value;
                    }

                    if (InputHelper.LeftMouseButtonReleased())
                    {
                        DeselectAll();

                        HideSelectionRectangle();

                        GetSelection(_selectionPreference, GetSelectedCells());
                    }

                    if (InputHelper.LeftMouseButtonIsBeingClicked())
                    {
                        ShowSelectionRectangle();

                        SelectionEndScreen = Input.mousePosition;
                        var start = Game.Instance.CameraController.Camera.WorldToScreenPoint(SelectionStartWorld);
                        start.z = 0f;

                        SelectSquareImage.position = (start + SelectionEndScreen) / 2;

                        var sizeX = Mathf.Abs(start.x - SelectionEndScreen.x);
                        var sizeY = Mathf.Abs(start.y - SelectionEndScreen.y);

                        SelectSquareImage.sizeDelta = new Vector2(sizeX, sizeY);
                    }
                }
            }
        }

        private void HideSelectionRectangle()
        {
            SelectSquareImage.gameObject.SetActive(false);
        }

        private bool MouseOverUi()
        {
            if (EventSystem.current == null)
            {
                // event system not on yet
                return false;
            }
            var overUI = EventSystem.current.IsPointerOverGameObject() && EventSystem.current.currentSelectedGameObject != null;

            if (overUI)
            {
                HideSelectionRectangle();
            }

            return overUI;
        }

        private void OnDrawGizmos()
        {
            var pos = GetWorldMousePosition().Value;

            Gizmos.color = ColorConstants.GreenAccent;
            var cell = Game.Instance.Map.GetCellAtCoordinate(pos);
            Gizmos.DrawCube(cell.Vector, new Vector3(1f, 0.01f, 1f));

            Gizmos.color = ColorConstants.RedAccent;
            Gizmos.DrawCube(pos, new Vector3(0.2f, 0.5f, 0.2f));
        }

        private bool SelectCreatures(List<CreatureRenderer> creatures)
        {
            foreach (var creature in creatures)
            {
                creature.EnableHighlight(ColorConstants.GreenAccent);
            }

            if (creatures?.Count > 0)
            {
                Game.Instance.ShowCreaturePanel(creatures);
                return true;
            }

            return false;
        }

        private bool SelectItems(List<Item> items)
        {
            foreach (var item in items)
            {
                item.ShowOutline();
            }

            if (items?.Count > 0)
            {
                Game.Instance.ShowItemPanel(items);
                return true;
            }
            return false;
        }

        private bool SelectStructures(List<Structure> structures)
        {
            foreach (var structure in structures)
            {
                structure.ShowOutline();
            }

            if (structures.Count > 0)
            {
                Game.Instance.ShowStructureInfoPanel(structures);
                return true;
            }
            return false;
        }

        private bool SelectZone(ZoneBase zone)
        {
            if (zone != null)
            {
                Game.Instance.ShowZonePanel(zone);
                return true;
            }
            return false;
        }

        private void ShowSelectionRectangle()
        {
            SelectSquareImage.gameObject.SetActive(true);
        }

        private void ValidateCursor(Cell startCell)
        {
            if (Validate?.Invoke(startCell) == false)
            {
                _meshRenderer?.SetAllMaterial(Game.Instance.FileController.InvalidBlueprintMaterial);
                if (MouseSpriteRenderer != null)
                {
                    MouseSpriteRenderer.color = ColorConstants.RedAccent;
                }
            }
            else
            {
                _meshRenderer?.SetAllMaterial(Game.Instance.FileController.BlueprintMaterial);
                if (MouseSpriteRenderer != null)
                {
                    MouseSpriteRenderer.color = ColorConstants.WhiteAccent;
                }
            }
        }
    }
}