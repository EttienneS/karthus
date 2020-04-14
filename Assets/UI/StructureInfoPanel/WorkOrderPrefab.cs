using Structures.Work;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WorkOrderPrefab : MonoBehaviour
    {
        public Image Background;
        public Text Description;
        public Image Image;
        public Text Title;

        public WorkDefinition Definition { get; set; }
        public WorkOption Option { get; set; }

        public WorkStructureBase Structure { get; set; }

        public void Click()
        {
            Game.Instance.StructureInfoPanel.SetSelected(this);
        }

        internal void Load(WorkStructureBase structure, WorkDefinition definition, WorkOption option)
        {
            Structure = structure;
            Definition = definition;
            Option = option;

            Image.sprite = Game.Instance.SpriteStore.GetSprite(Option.Sprite);
            Title.text = $"{definition.Name}: {option.Name}";
            Description.text = option.Description;
        }
    }
}