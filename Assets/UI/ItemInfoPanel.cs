using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ItemInfoPanel : MonoBehaviour
    {
        public Text Title;
        public Text Description;

        public List<Item> Items;

        public void Show(List<Item> items)
        {
            gameObject.SetActive(true);

            Items = items;

            var current = items[0];

            Title.text = current.Name;
            Description.text = string.Empty;

            Description.text += $"ID: {current.Id}\n";
            if (current.InUseByAnyone)
            {
                Description.text += $"In use by: {current.InUseBy.Name}\n";
            }
            Description.text += $"Amount: {current.Amount}\n";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}