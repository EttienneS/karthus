using UnityEngine;
using UnityEngine.UI;

public class OrderButton : MonoBehaviour
{
    public string Text
    {
        get
        {
            return _text.text;
        }
        set
        {
            _text.text = value;
        }
    }

    private Text _text;
    public Button Button;

    // Start is called before the first frame update
    private void Awake()
    {
        _text = GetComponentInChildren<Text>();
        Button = GetComponentInChildren<Button>();
    }
}