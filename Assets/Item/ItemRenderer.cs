using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace Assets.Item
{
    public class ItemRenderer : MonoBehaviour
    {
        public ItemData Data = new ItemData();

        private MeshRenderer _meshRenderer;
        private TextMeshPro _text;

        public void Start()
        {
            _text = Game.Instance.VisualEffectController.AddTextPrefab(gameObject);
            _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        }

        public void Update()
        {
            if (Data.Amount <= 0)
            {
                Game.Instance.ItemController.DestroyItem(Data);
            }
            else
            {
                _text.text = Data.Amount.ToString();
                _text.transform.localRotation = Quaternion.Euler(Game.Instance.CameraController.GetPerpendicularRotation(), -90, -90);
            }
        }

        internal void UpdatePosition()
        {
            transform.position = Data.Vector;
        }
    }
}