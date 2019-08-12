using UnityEngine;

public class Effect : MonoBehaviour
{
    internal float LifeSpan;

    private void Update()
    {
        LifeSpan -= Time.deltaTime;
        if (LifeSpan < 0)
        {
            Destroy(gameObject);
        }
    }
}