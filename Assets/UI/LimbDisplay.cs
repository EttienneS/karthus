using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LimbDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Text Title;
    public Image Background;

    public Limb Limb;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Limb != null)
        {
            Game.Instance.Tooltip.Show(Limb.Name, Limb.ToString());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Game.Instance.Tooltip.Hide();
    }

    private void Update()
    {
        Title.text = Limb.Name + " " + Limb.GetState();
        Background.color = Color.Lerp(Color.green, Color.red, Limb.GetChanceOfDeath() * 10);
    }
}