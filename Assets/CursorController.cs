using Assets.Helpers;
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
        public ValidateMouseDelegate Validate;
        internal MeshRenderer MeshRenderer;

        internal void ResetSelection()
        {
            Game.Instance.Cursor.SelectionStartWorld = Vector3.zero;
            Game.Instance.Cursor.SelectionPreference = SelectionPreference.Anything;
        }

        private Sprite _currentSprite;
        private readonly Dictionary<Cell, MeshRenderer> _draggedRenderers = new Dictionary<Cell, MeshRenderer>();
        private string _meshName;
        private RotateDelegate _rotateLeft;
        private RotateDelegate _rotateRight;

        public delegate void RotateDelegate();

        public delegate bool ValidateMouseDelegate(Cell cell);

        public RectTransform selectSquareImage;

        public void Start()
        {
            selectSquareImage.gameObject.SetActive(false);
        }

        public void Clear()
        {
            foreach (var meshRenderer in _draggedRenderers.Values)
            {
                Destroy(meshRenderer.gameObject);
            }
            _draggedRenderers.Clear();

            if (MeshRenderer != null)
            {
                Destroy(MeshRenderer.gameObject);
                MeshRenderer = null;
            }

            _meshName = string.Empty;

            if (_currentSprite != null)
            {
                MouseSpriteRenderer.sprite = null;
                _currentSprite = null;
            }
        }

        public void Disable()
        {
            MouseSpriteRenderer.sprite = null;
            MouseSpriteRenderer.size = Vector2.one;

            Clear();
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

            MeshRenderer = structure;
            Validate = validationFunction;
        }

        public void SetMultiSprite(Sprite sprite, ValidateMouseDelegate validationFunction)
        {
            SetSprite(sprite, validationFunction);

            var offsetX = ((sprite.texture.width / Map.PixelsPerCell) - 1) / 2f;
            var offsetZ = ((sprite.texture.height / Map.PixelsPerCell) - 1) / 2f;

            MouseSpriteRenderer.transform.localPosition = new Vector3(offsetX, 0.1f, offsetZ);
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

        public void SetSprite(Sprite sprite, ValidateMouseDelegate validationFunction)
        {
            MouseSpriteRenderer.sprite = sprite;
            Validate = validationFunction;

            _currentSprite = sprite;

            MouseSpriteRenderer.transform.localPosition = new Vector3(0f, 0.1f, 0f);
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

        internal void ShowConstructGhost(Construct construct)
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

        private void ValidateCursor(Cell startCell)
        {
            if (Validate?.Invoke(startCell) == false)
            {
                MeshRenderer?.SetAllMaterial(Game.Instance.FileController.InvalidBlueprintMaterial);
                if (MouseSpriteRenderer != null)
                {
                    MouseSpriteRenderer.color = ColorConstants.RedAccent;
                }
            }
            else
            {
                MeshRenderer?.SetAllMaterial(Game.Instance.FileController.BlueprintMaterial);
                if (MouseSpriteRenderer != null)
                {
                    MouseSpriteRenderer.color = ColorConstants.WhiteAccent;
                }
            }
        }

        #region selection logic isoloated for refactoring

        internal SelectionPreference LastSelection = SelectionPreference.Creature;
        internal List<Cell> SelectedCells = new List<Cell>();
        internal List<CreatureRenderer> SelectedCreatures = new List<CreatureRenderer>();
        internal List<Item> SelectedItems = new List<Item>();
        internal List<Structure> SelectedStructures = new List<Structure>();
        internal Vector3 SelectionEndScreen;
        internal SelectionPreference SelectionPreference;
        internal Vector3 SelectionStartWorld;

        public void DeselectAll()
        {
            Clear();
            DeselectCreature();
            DeselectCell();
            DeselectStructure();
            DeselectItem();
            DeselectZone();
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

            Game.Instance.DestroyCreaturePanel();
            Game.Instance.DestroyToolTip();
            SelectedCreatures.Clear();
        }

        public void DeselectItem()
        {
            foreach (var item in SelectedItems)
            {
                item.HideOutline();
            }
            SelectedItems.Clear();
            Game.Instance.DestroyItemInfoPanel();
        }

        public void DeselectStructure()
        {
            Disable();

            foreach (var structure in SelectedStructures)
            {
                structure.HideOutline();
            }
            Game.Instance.DestroyStructureInfoPanel();
            SelectedStructures.Clear();
        }

        public void DeselectZone()
        {
            Game.Instance.DestroyZonePanel();
        }

        public List<Cell> GetSelectedCells()
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

        public Vector3? GetWorldMousePosition()
        {
            var inputRay = Game.Instance.CameraController.Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(inputRay, out RaycastHit hit))
            {
                return hit.point;
            }
            return null;
        }

        public void HandleMouseInput()
        {
            if (Input.GetMouseButton(1))
            {
                // right mouse deselect all

                DeselectAll();
                Game.Instance.OrderSelectionController.DisableAndReset();
            }
            else
            {
                var mousePosition = GetWorldMousePosition();

                if (mousePosition != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (MouseOverUi())
                        {
                            return;
                        }
                        SelectionStartWorld = GetWorldMousePosition().Value;
                    }

                    if (Input.GetMouseButtonUp(0))
                    {
                        if (MouseOverUi())
                        {
                            return;
                        }

                        if (!Input.GetKey(KeyCode.LeftShift)
                            && !Input.GetKey(KeyCode.RightShift)
                            && Game.Instance.OrderSelectionController.CellClickOrder == null)
                        {
                            DeselectAll();
                        }

                        selectSquareImage.gameObject.SetActive(false);

                        SelectedCells = GetSelectedCells();

                        ZoneBase selectedZone = null;
                        foreach (var cell in SelectedCells)
                        {
                            if (cell.Structure != null)
                            {
                                SelectedStructures.Add(cell.Structure);
                            }
                            SelectedItems.AddRange(cell.Items);
                            SelectedCreatures.AddRange(cell.Creatures.Select(c => c.CreatureRenderer));

                            var zone = Game.Instance.ZoneController.GetZoneForCell(cell);
                            if (zone != null)
                            {
                                selectedZone = zone;
                            }
                        }

                        Select(selectedZone, SelectionPreference);

                        SelectionStartWorld = Vector3.zero;
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

                        SelectionEndScreen = Input.mousePosition;
                        var start = Game.Instance.CameraController.Camera.WorldToScreenPoint(SelectionStartWorld);
                        start.z = 0f;

                        selectSquareImage.position = (start + SelectionEndScreen) / 2;

                        var sizeX = Mathf.Abs(start.x - SelectionEndScreen.x);
                        var sizeY = Mathf.Abs(start.y - SelectionEndScreen.y);

                        selectSquareImage.sizeDelta = new Vector2(sizeX, sizeY);
                    }
                }
            }
        }

        public bool SelectCreature()
        {
            foreach (var creature in SelectedCreatures)
            {
                creature.EnableHighlight(ColorConstants.GreenAccent);
            }

            if (SelectedCreatures?.Count > 0)
            {
                DeselectCell();
                DeselectStructure();
                DeselectItem();
                DeselectZone();

                Game.Instance.ShowCreaturePanel(SelectedCreatures);
                return true;
            }

            return false;
        }

        internal void SelectZone(ZoneBase zone)
        {
            DeselectCell();
            DeselectCreature();
            DeselectStructure();
            DeselectItem();

            Game.Instance.ShowZonePanel(zone);
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
                selectSquareImage.gameObject.SetActive(false);
            }

            return overUI;
        }

        private bool Select(ZoneBase selectedZone, SelectionPreference selection)
        {
            switch (selection)
            {
                case SelectionPreference.Anything:

                    for (int i = 0; i < 5; i++)
                    {
                        switch (LastSelection)
                        {
                            case SelectionPreference.Creature:
                                LastSelection = SelectionPreference.Structure;
                                break;

                            case SelectionPreference.Structure:
                                LastSelection = SelectionPreference.Item;
                                break;

                            case SelectionPreference.Item:
                                LastSelection = SelectionPreference.Zone;
                                break;

                            case SelectionPreference.Zone:
                                LastSelection = SelectionPreference.Creature;
                                break;
                        }
                        if (Select(selectedZone, LastSelection))
                        {
                            break;
                        }
                    }
                    break;

                case SelectionPreference.Cell:
                    if (SelectedCells.Count > 0)
                    {
                        SelectCell();
                        return true;
                    }
                    break;

                case SelectionPreference.Item:
                    if (SelectedCells.Count > 0)
                    {
                        return SelectItem();
                    }
                    break;

                case SelectionPreference.Structure:
                    if (SelectedCells.Count > 0)
                    {
                        return SelectStructure();
                    }
                    break;

                case SelectionPreference.Creature:
                    if (SelectedCells.Count > 0)
                    {
                        return SelectCreature();
                    }
                    break;

                case SelectionPreference.Zone:
                    if (selectedZone != null)
                    {
                        SelectZone(selectedZone);
                        return true;
                    }
                    break;
            }
            return false;
        }

        private void SelectCell()
        {
            if (Game.Instance.OrderSelectionController.CellClickOrder != null)
            {
                //Debug.Log($"Clicked: {SelectedCells.Count}: {SelectedCells[0]}");
                Game.Instance.OrderSelectionController.CellClickOrder.Invoke(SelectedCells);
                DeselectCell();
            }
        }

        private bool SelectItem()
        {
            foreach (var item in SelectedItems)
            {
                item.ShowOutline();
            }

            if (SelectedItems?.Count > 0)
            {
                DeselectCell();
                DeselectStructure();
                DeselectCreature();
                DeselectZone();

                Game.Instance.ShowItemPanel(SelectedItems);
                return true;
            }
            return false;
        }

        private bool SelectStructure()
        {
            foreach (var structure in SelectedStructures)
            {
                structure.ShowOutline();
            }

            if (SelectedStructures?.Count > 0)
            {
                DeselectCell();
                DeselectItem();
                DeselectCreature();
                DeselectZone();

                Game.Instance.ShowStructureInfoPanel(SelectedStructures);
                return true;
            }
            return false;
        }

        #endregion selection logic isoloated for refactoring
    }
}