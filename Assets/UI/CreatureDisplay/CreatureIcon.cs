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
            Game.CameraController.MoveToCell(Creature.Cell);
        }
    }

    public void Start()
    {
        Image.sprite = Game.SpriteStore.GetSprite("Blank");
        Clothes.sprite = Game.SpriteStore.GetSprite("Blank");
        Text.text = "__";
    }

    private void Update()
    {
        if (Creature?.CreatureRenderer != null)
        {
            Image.sprite = Creature.CreatureRenderer.MainRenderer.sprite;
            Clothes.sprite = Creature.CreatureRenderer.ClothesRenderer.sprite;
            Text.text = Creature.Name;
        }
    }
}