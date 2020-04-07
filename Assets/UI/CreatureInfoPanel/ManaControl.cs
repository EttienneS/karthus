using UnityEngine;
using UnityEngine.UI;

public class ManaControl : MonoBehaviour
{
    public Text Title;
    public Slider Desired;
    public Slider Current;
    public Text Attunement;

    public ManaColor Color;

    private ManaPool _currentPool;

    public void SetPool(ManaPool manaPool)
    {
        _currentPool = manaPool;
        Desired.value = _currentPool[Color].Desired;
    }

    public void Awake()
    {
        Title.text = Color.ToString();

        foreach (var slider in GetComponentsInChildren<Slider>())
        {
            slider.handleRect.GetComponentInChildren<Image>().sprite = Game.SpriteStore.GetSprite(Color.ToString());
            slider.fillRect.GetComponentInChildren<Image>().color = Color.GetActualColor();
        }
    }

    public void Update()
    {
        if (_currentPool != null)
        {
            Desired.maxValue = _currentPool[Color].Attunement;
            Current.maxValue = _currentPool[Color].Attunement;

            _currentPool[Color].Desired = (int)Desired.value;
            Current.value = _currentPool[Color].Total;
            Attunement.text = _currentPool[Color].Attunement.ToString();
        }
    }
}