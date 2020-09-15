using Assets.Helpers;
using Assets.Item;
using Assets.Structures;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets
{
    public class CursorController : MonoBehaviour
    {
        public SpriteRenderer MouseSpriteRenderer;
        public RectTransform SelectSquareImage;
        public ValidateMouseDelegate Validate;

        private const int DoubleClickRadius = 10;
        private readonly Dictionary<Cell, MeshRenderer> _draggedRenderers = new Dictionary<Cell, MeshRenderer>();
        private Sprite _currentSprite;
        private SelectionPreference _lastSelection = SelectionPreference.Creature;
        private string _meshName;
        private MeshRenderer _meshRenderer;
        private RotateDelegate _rotateLeft;
        private RotateDelegate _rotateRight;
        private SelectionPreference _selectionPreference;
        private Vector3 _selectionStartWorld;

        public delegate void RotateDelegate();

        public delegate bool ValidateMouseDelegate(Cell cell);

        public void ResetSelection()
        {
            _selectionStartWorld = Vector3.zero;
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

        internal void Start()
        {
            HideSelectionRectangle();
        }

        internal void Update()
        {
            HandleMouseInput();

            MoveCursorTransform();
        }

        private static Cell GetCellForWorldPosition(Vector3? pos)
        {
            return Map.Instance.GetCellAtCoordinate(pos.Value - new Vector3(0.5f, 0, 0.5f));
        }

        private static void InvokeCellClickMethod(List<Cell> cells)
        {
            if (Game.Instance.OrderSelectionController.CellClickOrder != null)
            {
                Game.Instance.OrderSelectionController.CellClickOrder.Invoke(cells);
            }
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

        private void ClearSelection()
        {
            DeselectAll();
            HideSelectionRectangle();
        }

        private SelectionPreference CycleSelectionMode(SelectionPreference selection)
        {
            switch (selection)
            {
                case SelectionPreference.Creature:
                    return SelectionPreference.Structure;

                case SelectionPreference.Structure:
                    return SelectionPreference.Item;

                case SelectionPreference.Item:
                    return SelectionPreference.Zone;

                case SelectionPreference.Zone:
                    return SelectionPreference.Creature;

                default:
                    throw new System.Exception("Unkown selection type!");
            }
        }

        private void DeselectAll()
        {
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
            foreach (var item in Game.Instance.IdService.ItemIdLookup.Values)
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

        private List<ItemData> FindItemsInCells(List<Cell> cells)
        {
            var items = new List<ItemData>();
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
            if (_selectionStartWorld == Vector3.zero)
            {
                return new List<Cell>();
            }
            var worldStartPoint = _selectionStartWorld;
            var worldEndPoint = GetWorldMousePosition().Value;

            var cells = new List<Cell>();

            var startX = Mathf.Clamp(Mathf.Min(worldStartPoint.x, worldEndPoint.x), Map.Instance.MinX, Map.Instance.MaxX);
            var endX = Mathf.Clamp(Mathf.Max(worldStartPoint.x, worldEndPoint.x), Map.Instance.MinX, Map.Instance.MaxX);

            var startZ = Mathf.Clamp(Mathf.Min(worldStartPoint.z, worldEndPoint.z), Map.Instance.MinZ, Map.Instance.MaxZ);
            var endZ = Mathf.Clamp(Mathf.Max(worldStartPoint.z, worldEndPoint.z), Map.Instance.MinX, Map.Instance.MaxZ);

            if (startX == endX && startZ == endZ)
            {
                var point = new Vector3(startX, 0, startZ);

                cells.Add(GetCellForWorldPosition(point));
            }
            else
            {
                for (var selX = startX; selX < endX; selX++)
                {
                    for (var selZ = startZ; selZ < endZ; selZ++)
                    {
                        var point = new Vector3(selX, 0, selZ);
                        cells.Add(GetCellForWorldPosition(point));
                    }
                }
            }

            return cells.Distinct().ToList();
        }

        private bool GetSelection(SelectionPreference selectionPreference, List<Cell> cells, bool selectSimilar)
        {
            _selectionStartWorld = Vector3.zero;

            switch (selectionPreference)
            {
                case SelectionPreference.Anything:

                    for (int i = 0; i < 5; i++)
                    {
                        _lastSelection = CycleSelectionMode(_lastSelection);
                        if (GetSelection(_lastSelection, cells, selectSimilar))
                        {
                            break;
                        }
                    }
                    break;

                case SelectionPreference.Cell:
                    if (cells.Count > 0)
                    {
                        InvokeCellClickMethod(cells);
                        return true;
                    }
                    break;

                case SelectionPreference.Item:
                    return SelectItems(FindItemsInCells(cells), selectSimilar);

                case SelectionPreference.Structure:
                    return SelectStructures(FindStructuresInCells(cells), selectSimilar);

                case SelectionPreference.Creature:
                    return SelectCreatures(FindCreaturesInCells(cells), selectSimilar);

                case SelectionPreference.Zone:
                    return SelectZone(FindZoneInCells(cells), selectSimilar);
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
                Clear();
                DeselectAll();
                Game.Instance.OrderSelectionController.DisableAndReset();
            }
            else
            {
                var worldMousePosition = GetWorldMousePosition();

                if (worldMousePosition != null)
                {
                    if (InputHelper.LeftMouseButtonDoubleClicked())
                    {
                        ClearSelection();
                        GetSelection(_selectionPreference, GetSelectedCells(), true);
                    }
                    else
                    {
                        if (InputHelper.LeftMouseButtonStartClick())
                        {
                            _selectionStartWorld = GetWorldMousePosition().Value;
                        }

                        if (InputHelper.LeftMouseButtonReleased())
                        {
                            ClearSelection();
                            GetSelection(_selectionPreference, GetSelectedCells(), false);
                        }

                        if (InputHelper.LeftMouseButtonIsBeingClicked())
                        {
                            ShowSelectionRectangle();

                            var selectionEndScreenPosition = Input.mousePosition;
                            var start = Game.Instance.CameraController.Camera.WorldToScreenPoint(_selectionStartWorld);
                            start.z = 0f;

                            SelectSquareImage.position = (start + selectionEndScreenPosition) / 2;

                            var sizeX = Mathf.Abs(start.x - selectionEndScreenPosition.x);
                            var sizeY = Mathf.Abs(start.y - selectionEndScreenPosition.y);

                            SelectSquareImage.sizeDelta = new Vector2(sizeX, sizeY);

                            ShowCursorEffects(GetSelectedCells());
                        }
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

        private void MoveCursorTransform()
        {
            var pos = GetWorldMousePosition();
            if (pos != null)
            {
                var startCell = GetCellForWorldPosition(pos);
                if (startCell == null)
                {
                    return;
                }
                transform.position = new Vector3(startCell.Vector.x,
                                                 startCell.Vector.y,
                                                 startCell.Vector.z) + new Vector3(0, 0.25f, 0);

                ValidateCursor(startCell);
            }
        }

        private void OnDrawGizmos()
        {
            var worldPos = GetWorldMousePosition();

            if (worldPos.HasValue)
            {
                var pos = worldPos.Value;

                Gizmos.color = new Color(0, 1, 0, 0.2f);

                var cell = GetCellForWorldPosition(pos);

                if (cell != null)
                {
                    Gizmos.DrawCube(cell.Vector, new Vector3(1f, 0.01f, 1f));
                }
            }

            Gizmos.DrawCube(_selectionStartWorld, new Vector3(1f, 0.01f, 1f));
        }

        private bool SelectCreatures(List<CreatureRenderer> creatures, bool selectSimilar)
        {
            if (creatures.Count == 1 && selectSimilar)
            {
                var creature = creatures[0].Data;
                creatures = Map.Instance.GetCircle(creature.Cell, DoubleClickRadius)
                                        .SelectMany(c => c.Creatures)
                                        .Where(c => c.BehaviourName == creature.BehaviourName)
                                        .Select(c => c.CreatureRenderer)
                                        .ToList();
            }

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

        private bool SelectItems(List<ItemData> items, bool selectSimilar)
        {
            if (items.Count == 1 && selectSimilar)
            {
                var item = items[0];
                items = Map.Instance.GetCircle(item.Cell, DoubleClickRadius)
                                    .SelectMany(c => c.Items)
                                    .Where(i => i.Name == item.Name)
                                    .ToList();
            }

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

        private bool SelectStructures(List<Structure> structures, bool selectSimilar)
        {
            if (structures.Count == 1 && selectSimilar)
            {
                var structure = structures[0];
                structures = Map.Instance.GetCircle(structure.Cell, DoubleClickRadius)
                                         .Select(c => c.Structure)
                                         .Where(s => s != null && s.Name == structure.Name)
                                         .ToList();
            }

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

        private bool SelectZone(ZoneBase zone, bool selectSimilar)
        {
            if (zone != null)
            {
                Game.Instance.ShowZonePanel(zone);
                return true;
            }
            return false;
        }

        private void ShowCursorEffects(List<Cell> selectedCells)
        {
            if (!string.IsNullOrEmpty(_meshName))
            {
                if (selectedCells == null || selectedCells.Count == 0)
                {
                    return;
                }

                foreach (var cell in selectedCells)
                {
                    MeshRenderer cellRenderer;
                    if (!_draggedRenderers.ContainsKey(cell))
                    {
                        cellRenderer = Game.Instance.StructureController.InstantiateNewStructureMeshRenderer(_meshName, Map.Instance.transform);
                        cellRenderer.transform.position = new Vector3(cell.Vector.x, cell.Vector.y, cell.Vector.z);
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