using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace Assets.Item
{
    public class ItemRenderer : MonoBehaviour
    {
        internal ItemData Data = new ItemData();

        private MeshRenderer _meshRenderer;
        //private TextMeshPro _text;

        public void Start()
        {
            //_text = GetComponentInChildren<TextMeshPro>();
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
                //_text.text = Data.Amount.ToString();
            }
        }

        internal void UpdatePosition()
        {
            transform.position = Data.Vector;
        }
    }
}