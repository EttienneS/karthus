using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : MonoBehaviour
{
    private bool _bluePrint;

    internal List<Item> ContainedItems = new List<Item>();

    internal bool BluePrint
    {
        get
        {
            return _bluePrint;
        }
        set
        {
            _bluePrint = value;

            if (_bluePrint)
            {
                SpriteRenderer.color = new Color(0.3f, 1f, 1f, 0.6f);
                SpriteRenderer.material.SetFloat("_EffectAmount", 1f);
            }
            else
            {
                SpriteRenderer.color = Color.white;
                SpriteRenderer.material.SetFloat("_EffectAmount", 0f);
            }
        }
    }

    internal SpriteRenderer SpriteRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (BluePrint)
        {
            if (!Taskmaster.Instance.ContainsJob(name))
            {
                Taskmaster.Instance.AddTask(new Build(this, GetComponentInParent<Cell>()));
            }
        }
    }
}
