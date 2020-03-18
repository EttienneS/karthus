using UnityEngine;
using UnityEngine.UI;

public class CreatureIcon : MonoBehaviour
{
    public Image Image;
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
            Text.text = Creature.Name;
        }
    }
}