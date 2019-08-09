using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    public ParticleSystem EffectPrefab;

    public Dictionary<ParticleSystem, float> ActiveEffects = new Dictionary<ParticleSystem, float>();

    // Update is called once per frame
    public void Update()
    {
        //foreach (var effect in ActiveEffects)
        //{
        //    effect.Value -= Time.deltaTime;
        //}
    }

    public void SpawnEffect(Coordinates coordinates, float lifeSpan)
    {
        var effect = Instantiate(EffectPrefab,transform);
        ActiveEffects.Add(effect, lifeSpan);
        effect.transform.position = coordinates.ToTopOfMapVector();
    }
}
