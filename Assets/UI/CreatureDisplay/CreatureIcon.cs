using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Creature;
using Assets.ServiceLocator;

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
            Loc.GetCamera().ViewPoint(Creature.Vector);
            Loc.GetGameController().ShowCreaturePanel(new List<CreatureRenderer> { Creature.CreatureRenderer });
        }
    }

    public void Start()
    {
        Image.sprite = Loc.GetSpriteStore().GetSprite("Blank");
        Clothes.sprite = Loc.GetSpriteStore().GetSprite("Blank");
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