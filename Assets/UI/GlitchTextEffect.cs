using System.Linq;
using TMPro;
using UnityEngine;

public static class StringExtension
{
    private static char[] _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz".ToArray();

    public static char GetRandomCharacter()
    {
        return _chars.GetRandomItem();
    }
}

public class GlitchTextEffect : MonoBehaviour
{
    private string _baseText;
    private TextMeshProUGUI _textRenderer;
    private float _updateDelta;

    private float _updateTime;

    private void ResetUpdateTime()
    {
        _updateDelta = 0;
        _updateTime = Random.Range(0.15f, 0.3f);
    }

    private void Start()
    {
        _textRenderer = GetComponentInChildren<TextMeshProUGUI>();
        _baseText = _textRenderer.text;
        ResetUpdateTime();
    }

    private void Update()
    {
        _updateDelta += Time.deltaTime;

        if (_updateDelta > _updateTime)
        {
            ResetUpdateTime();

            if (!_revert)
            {
                Change();
            }
            else
            {
                _textRenderer.text = _baseText;
                _updateTime = Random.Range(1f, 2f);
            }

            if (Random.value > 0.5f)
            {
                _revert = !_revert;
            }
        }
    }

    private void Change()
    {
        var changeCount = Mathf.Max(Random.Range(1, 3), (int)(_baseText.Length * 0.1f));
        var newText = _textRenderer.text;
        for (int i = 0; i < changeCount; i++)
        {
            var charIndex = Random.Range(0, _baseText.Length);

            var value = StringExtension.GetRandomCharacter().ToString();
            newText = newText.Remove(charIndex, 1);
            newText = newText.Insert(charIndex, value);
        }

        _textRenderer.text = newText;
    }

    private bool _revert;
}