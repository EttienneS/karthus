using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatureIcon : MonoBehaviour
{
    public Image Image;
    public Image Clothes;
    public Text Text;

    public Creature Creature { get; set; }

    public void CreatureClicked()
    {
        if (Creature != null)
        {
            Game.Instance.CameraController.MoveToCell(Creature.Cell);
            Game.Instance.SelectedCreatures = new List<CreatureRenderer> { Creature.CreatureRenderer };
            Game.Instance.SelectCreature();
        }
    }

    public void Start()
    {
        Image.sprite = Game.Instance.SpriteStore.GetSprite("Blank");
        Clothes.sprite = Game.Instance.SpriteStore.GetSprite("Blank");
        Text.text = "__";
    }

    private void Update()
    {
        if (Creature?.CreatureRenderer != null)
        {
            Text.text = Creature.Name;
        }
    }
}