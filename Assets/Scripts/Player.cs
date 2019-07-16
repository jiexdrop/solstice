using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject center;
    public GameObject visor;

    public float visorRotation;

    void Start()
    {
        center = transform.GetChild(0).gameObject;
        visor = center.transform.GetChild(0).gameObject;
    }

    void Update()
    {

    }

    internal void SetRotation(Vector2 inputVector)
    {
        visorRotation = -Mathf.Atan2(inputVector.x, inputVector.y) * Mathf.Rad2Deg + 90;
        center.transform.rotation = Quaternion.Euler(0, 0, visorRotation);
    }

    internal void SetRotation(float visorRotation)
    {
        this.visorRotation = visorRotation;
        center.transform.rotation = Quaternion.Euler(0, 0, visorRotation);
    }
}
