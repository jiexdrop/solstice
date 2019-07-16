﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float duration;

    private float elapsed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;

        // transform.right = transform.forward for 2D
        transform.position += transform.right * Time.deltaTime * 5; 

        if(elapsed > duration)
        {
            Destroy(this.gameObject);
            Destroy(this);
        }
    }
}