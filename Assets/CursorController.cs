using Assets.Helpers;
using Boo.Lang;
using UnityEngine;

namespace Assets
{
    public class CursorController : MonoBehaviour
    {
        public Light CursorLight;
        public MeshRenderer MouseMeshRenderer;
        public SpriteRenderer MouseSpriteRenderer;

        public RotateDelegate RotateLeft;
        public RotateDelegate RotateRight;
        public ValidateMouseDelegate Validate;

        public delegate void RotateDelegate();

        public delegate bool ValidateMouseDelegate(Cell cell);

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
            var structure = Game.Instance.StructureController.GetMeshForStructure(name, transform);
            MouseMeshRenderer = structure.renderer;

            MouseMeshRenderer.SetAllMaterial(Game.Instance.FileController.BlueprintMaterial);

            _offset = structure.structure.OffsetVector;
        }

       

        private void DisableMesh()
        {
            if (MouseMeshRenderer != null)
            {
                Destroy(MouseMeshRenderer.gameObject);
            }
        }

        public void SetSprite(Sprite sprite, ValidateMouseDelegate validationFunction)
        {
        }

        private Vector3 _offset = Vector3.zero;

        public void Update()
        {
            CursorLight.enabled = Game.Instance.TimeManager.Data.Hour > 17 || Game.Instance.TimeManager.Data.Hour < 5;

            var pos = Game.Instance.GetWorldMousePosition();
            if (pos != null)
            {
                var cell = Game.Instance.Map.GetCellAtPoint(pos.Value);

                if (cell == null)
                {
                    return;
                }

                float x = cell.X;
                float z = cell.Z;

                transform.position = new Vector3(x, cell.Y, z) + _offset + new Vector3(0.5f, 0.5f, 0.5f);

                //if (Cursor.MouseSpriteRenderer.sprite != null)
                //{
                //    if (ValidateMouse != null)
                //    {
                //        if (!ValidateMouse(cell))
                //        {
                //            Cursor.MouseSpriteRenderer.color = ColorConstants.RedBase;
                //        }
                //        else
                //        {
                //            Cursor.MouseSpriteRenderer.color = ColorConstants.BluePrintColor;
                //        }
                //    }

                //    if (SelectionStartWorld != Vector3.zero && !_constructMode)
                //    {
                //        Cursor.MouseSpriteRenderer.color = new Color(0, 0, 0, 0);
                //        ClearGhostEffects();
                //        foreach (var c in GetSelectedCells(SelectionStartWorld, GetWorldMousePosition().Value))
                //        {
                //            var color = ColorConstants.BluePrintColor;
                //            if (ValidateMouse != null && !ValidateMouse(c))
                //            {
                //                color = ColorConstants.RedBase;
                //            }

                //            _ghostEffects.Add(VisualEffectController.SpawnSpriteEffect(null, c.Vector, MouseSpriteName, float.MaxValue, color));
                //        }
                //    }
                //}
            }
        }

        internal void ShowConstructGhost(Construct construct)
        {
            RotateLeft = () => construct.RotateRight();
            RotateRight = () => construct.RotateLeft();
            Validate = (cell) => construct.ValidateStartPos(cell);
        }
    }
}