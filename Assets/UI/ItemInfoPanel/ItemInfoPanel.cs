using Assets.Item;
using Assets.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ItemInfoPanel : MonoBehaviour
    {
        public Text Title;
        public Text Description;

        public List<ItemData> Items;

        public void Show(List<ItemData> items)
        {
            gameObject.SetActive(true);

            Items = items;
        }

        public void StoreItems()
        {
            foreach (var item in Items)
            {
                Game.Instance.FactionController.PlayerFaction.AddTask(new StoreItem(item));
            }
        }

        public void Update()
        {
            var current = Items[0];

            Title.text = current.Name;
            Description.text = string.Empty;

            Description.text += $"ID: {current.Id}\n";
            if (current.InUseByAnyone)
            {
                Description.text += $"In use by: {current.InUseById.GetEntity().Name}\n";
            }
            Description.text += $"Amount: {current.Amount}\n";

            Description.text += $"\n{current.Cell}\n";
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}