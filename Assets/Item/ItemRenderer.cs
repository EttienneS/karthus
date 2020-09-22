using Assets.ServiceLocator;
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
            _text = Loc.GetVisualEffectController().AddTextPrefab(gameObject);
            _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        }

        public void Update()
        {
            if (Data.Amount <= 0)
            {
                Loc.GetItemController().DestroyItem(Data);
            }
            else
            {
                _text.text = Data.Amount.ToString();
                _text.transform.localRotation = Quaternion.Euler(Loc.GetCamera().GetPerpendicularRotation(), -90, -90);
            }
        }

        internal void UpdatePosition()
        {
            transform.position = Data.Vector;
        }
    }
}