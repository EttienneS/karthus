using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    public Effect EffectPrefab;

    public void SpawnEffect(Coordinates coordinates, float lifeSpan)
    {
        var effect = Instantiate(EffectPrefab, transform);
        effect.LifeSpan = lifeSpan;
        effect.transform.position = coordinates.ToTopOfMapVector();
    }
}
