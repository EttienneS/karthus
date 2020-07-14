using Assets.Helpers;
using Assets.Structures;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public class CursorController : MonoBehaviour
    {
        public Light CursorLight;
        public SpriteRenderer MouseSpriteRenderer;
        public ValidateMouseDelegate Validate;
        internal MeshRenderer MeshRenderer;
        private Sprite _currentSprite;
        private Dictionary<Cell, MeshRenderer> _draggedRenderers = new Dictionary<Cell, MeshRenderer>();
        private string _meshName;
        private RotateDelegate _rotateLeft;
        private RotateDelegate _rotateRight;

        public delegate void RotateDelegate();

        public delegate bool ValidateMouseDelegate(Cell cell);

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
    }
}