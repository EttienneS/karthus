using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Creature;

public class CreatureIcon : MonoBehaviour
{
    public Image Image;
    public Image Clothes;
    public Text Text;

    public CreatureData Creature { get; set; }

    public void CreatureClicked()
    {
        if (Creature != null)
        {
            Game.Instance.CameraController.ViewPoint(Creature.Vector);
            Game.Instance.ShowCreaturePanel(new List<CreatureRenderer> { Creature.CreatureRenderer });
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