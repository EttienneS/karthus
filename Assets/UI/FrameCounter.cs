using System.Linq;
using TMPro;
using UnityEngine;

public class FrameCounter : MonoBehaviour
{
    public float RefreshInterval = 1f;
    private const int _bufferSize = 1000;

    private float[] _buffer;
    private int _current;
    private float _lastUpdate;
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _buffer = new float[_bufferSize];
    }

    private void Update()
    {
        var fps = Mathf.RoundToInt(1 / Time.deltaTime * Time.timeScale);
        _buffer[_current] = fps;
        _current = (_current + 1) % _bufferSize;

        _lastUpdate += Time.deltaTime;
        if (_lastUpdate > RefreshInterval)
        {
            _lastUpdate = 0;

            var min = _buffer.Min();
            var max = _buffer.Max();
            var avg = Mathf.RoundToInt(_buffer.Average());

            _text.text = $"FPS {fps}\nAVG {avg}\nMIN {min}\nMAX {max}";
        }
    }
}