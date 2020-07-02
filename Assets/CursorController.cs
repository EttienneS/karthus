using Assets.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public class CursorController : MonoBehaviour
    {
        public Light CursorLight;
        public SpriteRenderer MouseSpriteRenderer;
        public RotateDelegate RotateLeft;
        public RotateDelegate RotateRight;
        public ValidateMouseDelegate Validate;

        internal MeshRenderer MeshRenderer;
        private Sprite _currentSprite;
        private Dictionary<Cell, MeshRenderer> _draggedRenderers = new Dictionary<Cell, MeshRenderer>();
        private string _meshName;

        private Vector3 _offset = Vector3.zero;

        public delegate void RotateDelegate();

        public delegate bool ValidateMouseDelegate(Cell cell);

        public void Clear()
        {
            if (MeshRenderer != null)
            {
                Destroy(MeshRenderer.gameObject);
            }
        }

        public void Disable()
        {
            MouseSpriteRenderer.sprite = null;
            MouseSpriteRenderer.size = Vector2.one;
            //ValidateMouse = null;
            //RotateMouseRight = null;

            DisableMesh();
        }

        public void Enable()
        {
        }

        public void SetMesh(string name, ValidateMouseDelegate validationFunction)
        {
            DisableMesh();
            _meshName = name;
            Clear();

            var structure = Game.Instance.StructureController.GetMeshForStructure(name, transform);
            _offset = structure.structure.OffsetVector;

            structure.renderer.SetAllMaterial(Game.Instance.FileController.BlueprintMaterial);

            MeshRenderer = structure.renderer;
            Validate = validationFunction;
        }

        public void SetSprite(Sprite sprite, ValidateMouseDelegate validationFunction)
        {
            MouseSpriteRenderer.sprite = sprite;
            Validate = validationFunction;

            _currentSprite = sprite;
        }

        public void Update()
        {
            CursorLight.enabled = Game.Instance.TimeManager.Data.Hour > 17 || Game.Instance.TimeManager.Data.Hour < 5;

            var pos = Game.Instance.GetWorldMousePosition();
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
                var selectedCells = Game.Instance.GetSelectedCells();

                if (selectedCells == null || selectedCells.Count == 0)
                {
                    return;
                }

                foreach (var cell in selectedCells)
                {
                    MeshRenderer cellRenderer;
                    if (!_draggedRenderers.ContainsKey(cell))
                    {
                        cellRenderer = Game.Instance.StructureController.GetMeshForStructure(_meshName, Game.Instance.Map.transform).renderer;
                        cellRenderer.transform.position = new Vector3(cell.Vector.x, Game.Instance.MapData.StructureLevel, cell.Vector.z) + _offset;
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

        internal void ShowConstructGhost(Construct construct)
        {
            RotateLeft = () => construct.RotateRight();
            RotateRight = () => construct.RotateLeft();
            Validate = (cell) => construct.ValidateStartPos(cell);
        }

        private void DisableMesh()
        {
            if (MeshRenderer != null)
            {
                foreach (var meshRenderer in _draggedRenderers.Values)
                {
                    Destroy(meshRenderer.gameObject);
                }
                _draggedRenderers.Clear();
                _meshName = string.Empty;
            }

            if (_currentSprite != null)
            {
                MouseSpriteRenderer.sprite = null;
                _currentSprite = null;
            }
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
    }
}