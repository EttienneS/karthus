using UnityEngine;

public class TitleBackground : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        //_spriteRenderer.transform.position += new Vector3(1f * Time.deltaTime, 1f * Time.deltaTime, 0);
        _spriteRenderer.transform.eulerAngles += new Vector3(0, 0, 0.25f * Time.deltaTime);
    }
}