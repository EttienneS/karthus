using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    internal bool Reserved;
    internal SpriteRenderer SpriteRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

  
}
