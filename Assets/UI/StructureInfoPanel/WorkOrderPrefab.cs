using Assets.ServiceLocator;
using Structures.Work;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WorkOrderPrefab : MonoBehaviour
    {
        public Image Background;
        public Image Image;
        public Text Title;

        public WorkDefinition Definition { get; set; }
        public WorkOption Option { get; set; }

        public WorkStructureBase Structure { get; set; }

        public StructureInfoPanel Panel { get; set; }

        public void Click()
        {
            Panel.SetSelected(this);
        }

        internal void Load(WorkStructureBase structure, WorkDefinition definition, WorkOption option, StructureInfoPanel panel)
        {
            Panel = panel;
            Structure = structure;
            Definition = definition;
            Option = option;

            Image.sprite = Loc.GetSpriteStore().GetSprite(Option.Icon);
            Title.text = $"{definition.Name}: {option.Name}";
        }
    }
}