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

    private void Update()
    {
        if (Creature != null)
        {
            Image.sprite = Creature.CreatureRenderer.MainRenderer.sprite;
            Clothes.sprite = Creature.CreatureRenderer.ClothesRenderer.sprite;
            Text.text = Creature.Name;
        }
    }
}