using System.Collections.Generic;
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
    private List<int> _changeIndexes = new List<int>();
    private bool _revert;
    private TextMeshProUGUI _textRenderer;
    private float _updateDelta;
    private float _updateTime;
    private int _changesBeforeRevert;

    private void Change()
    {
        if (_changeIndexes.Count == 0)
        {
            var changeCount = Mathf.Max(Random.Range(1, 3), (int)(_baseText.Length * 0.1f));
            for (int i = 0; i < changeCount; i++)
            {
                _changeIndexes.Add(Random.Range(0, _baseText.Length));
            }
            _changesBeforeRevert = Random.Range(4, 8);
        }
        else
        {
            var newText = _textRenderer.text;

            foreach (var index in _changeIndexes)
            {
                var value = StringExtension.GetRandomCharacter().ToString();
                newText = newText.Remove(index, 1);
                newText = newText.Insert(index, value);
            }

            _textRenderer.text = newText;

            _changesBeforeRevert--;
            if (_changesBeforeRevert <= 0)
            {
                _revert = true;
            }
        }
    }

    private void ResetUpdateTime()
    {
        _updateDelta = 0;
        _updateTime = Random.Range(0.025f, 0.05f);
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
                _updateTime = Random.Range(3f, 5f);
                _changeIndexes.Clear();
                _revert = false;
            }
        }
    }
}