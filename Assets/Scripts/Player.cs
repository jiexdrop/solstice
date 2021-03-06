﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public GameObject center;
    public GameObject visor;
    public GameObject shootExit;

    public Animator animator;

    public float visorRotation;

    [Header("Health")]
    public Slider healthBar;
    public int health = GameManager.MAX_HEALTH;

    public bool isPlayed; // I'm currently being played or do my values come from the server

    public bool died;

    public Server server;
    public Client client;

    [Header("Weapon")]
    public float frequency = 0.2f;    
    public int dammage = 1;
    public Pickable.Type type = Pickable.Type.PISTOL;
    public AnimationCurve recoilCurve;

    public bool shooting;
    public float shootingElapsed;

    private BoxCollider2D swordCollider;

    void Start()
    {
        center = transform.GetChild(0).gameObject;
        swordCollider = center.GetComponent<BoxCollider2D>();
        swordCollider.enabled = false;
        visor = center.transform.GetChild(0).gameObject;
        shootExit = visor.transform.GetChild(0).gameObject;

        animator = GetComponent<Animator>();
        recoilCurve.postWrapMode = WrapMode.Loop;

        saveVisorPosition = new Vector3(0.7f, 0, 0);
    }

    public Vector2 GetVisorDirection()
    {
        if (visor.transform.rotation.eulerAngles.z >= 45 && visor.transform.rotation.eulerAngles.z < 135)
        {
            // TOP
            return new Vector2(0, 1);
        }

        if (visor.transform.rotation.eulerAngles.z >= 225 && visor.transform.rotation.eulerAngles.z < 315)
        {
            // BOTTOM
            return new Vector2(0, -1);
        }

        if (visor.transform.rotation.eulerAngles.z >= 135 && visor.transform.rotation.eulerAngles.z < 225)
        {
            // LEFT
            return new Vector2(-1, 0);
        }

        if ((visor.transform.rotation.eulerAngles.z >= 315 && visor.transform.rotation.eulerAngles.z < 360)
            || (visor.transform.rotation.eulerAngles.z < 45 && visor.transform.rotation.eulerAngles.z >= 0))
        {
            // RIGHT
            return new Vector2(1, 0);
        }

        return new Vector2(1, 0);
    }

    internal void UsePickable(Pickable pickable)
    {
        switch (pickable.type)
        {
            case Pickable.Type.HEALTH_POTION:
                IncrementHealth(3);
                break;
            case Pickable.Type.ENERYGY_POTION:
                IncrementHealth(1);
                break;
            case Pickable.Type.POISON_POTION:
                health--; // Don't dysplay you've been poisoned
                break;

            case Pickable.Type.KATANA:
                frequency = 0.4f;
                dammage = 3;
                visor.GetComponent<SpriteRenderer>().sprite = pickable.sprites[(int)pickable.type];
                visor.transform.localRotation = Quaternion.identity;
                shootExit.transform.localPosition = new Vector3(0.6f, 0.0f, 0);
                type = pickable.type;
                break;
            case Pickable.Type.PISTOL:
                frequency = 0.2f;
                dammage = 1;
                visor.GetComponent<SpriteRenderer>().sprite = pickable.sprites[(int)pickable.type];
                visor.transform.localRotation = Quaternion.identity;
                shootExit.transform.localPosition = new Vector3(0.3f, 0.07f, 0);
                type = pickable.type;
                break;
            case Pickable.Type.BOW:
                frequency = 0.4f;
                dammage = 2;
                visor.GetComponent<SpriteRenderer>().sprite = pickable.sprites[(int)pickable.type];
                visor.transform.localRotation = Quaternion.Euler(0, 0, -45);
                shootExit.transform.localPosition = new Vector3(0.3f, 0.3f, 0);
                type = pickable.type;
                break;
        }
    }

    public Vector2 GetRotatedVisorDirection(int playerId)
    {
        Vector2 visorRotation = GetVisorDirection();
        float angle = 0;
        switch (playerId)
        {
            case 0:
                angle = -15;
                break;
            case 1:
                angle = 0;
                break;
            case 2:
                angle = 15;
                break;
            case 3:
                angle = 30;
                break;
            default:
                Debug.LogError("Impossible player id: " + playerId);
                break;
        }

        angle = angle * Mathf.Deg2Rad;
        visorRotation.x = (visorRotation.x * Mathf.Cos(angle)) - (visorRotation.y * Mathf.Sin(angle));
        visorRotation.y = (visorRotation.x * Mathf.Sin(angle)) + (visorRotation.y * Mathf.Cos(angle));

        return visorRotation;
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

    public void SetDied()
    {
        died = true;
        if (isPlayed)
        {
            if(server != null)
            {
                server.ShareDeath(0);
            }

            if(client != null)
            {
                client.ShareDeath(client.playerId);
            }
            Camera.main.transform.parent = transform.parent;
        }
        animator.enabled = false;
        GetComponent<SpriteRenderer>().color = Color.black;
        transform.rotation = Quaternion.Euler(0, 0, 90);
    }

    public Vector3 backwardsVisorPosition;
    public float topVisorRotation;
    public float bottomVisorRotation;
    public Vector3 saveVisorPosition;
    private bool startShooting;

    internal void StartShooting()
    {
        topVisorRotation = center.transform.localRotation.eulerAngles.z - 45;
        bottomVisorRotation = center.transform.localRotation.eulerAngles.z + 45;
        switch(type)
        {
            case Pickable.Type.KATANA:
                swordCollider.enabled = true;
                break;
        }
    }

    internal void AnimateShooting(float shootingElapsed)
    {
        backwardsVisorPosition = visor.transform.localPosition * 0.25f;

        if (!startShooting)
        {
            startShooting = true;
            StartShooting();
        }

        //Debug.Log(recoilCurve.Evaluate(Time.time * 1/frequency));
        //Debug.Log("shootingElapsed " + shootingElapsed);
        
        switch (type)
        {
            case Pickable.Type.PISTOL:
                {
                    float lerpPercent = recoilCurve.Evaluate(Time.time * 1 / frequency);
                    visor.transform.localPosition = Vector2.Lerp(saveVisorPosition, backwardsVisorPosition, lerpPercent);
                }
                break;
            case Pickable.Type.BOW:
                {
                    float lerpPercent = recoilCurve.Evaluate(Time.time * 1 / frequency);
                    visor.transform.localPosition = Vector2.Lerp(saveVisorPosition, backwardsVisorPosition, lerpPercent);
                }
                break;
            case Pickable.Type.KATANA:
                {
                    float lerpPercent = recoilCurve.Evaluate(Time.time * 1 / frequency);
                    SetRotation(Mathf.Lerp(topVisorRotation, bottomVisorRotation, lerpPercent));
                }
                break;
        }

        if (shootingElapsed >= frequency)
        {
            //Debug.Log("End animate shooting");
            //visor.transform.localPosition = saveVisorPosition;
        }
    }

    public void IncrementHealth(int points)
    {
        if (health + points < GameManager.MAX_HEALTH)
        {
            health += points;
        } else
        {
            health = GameManager.MAX_HEALTH;
        }
        if (healthBar != null) // if player has healthbar visible
        {
            healthBar.value = health;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "MonsterProjectile")
        {
            Destroy(collision.gameObject);
            if (isPlayed)
            {
                health--;
                healthBar.value = health;
                if (health < 0)
                {
                    SetDied();
                }
            }
        }

        if(collision.gameObject.tag == "Chest")
        {
            collision.gameObject.GetComponent<Chest>().OpenChest();
        }
    }

    public void StopShooting()
    {
        visor.transform.localPosition = saveVisorPosition;
        startShooting = false;
        switch (type)
        {
            case Pickable.Type.KATANA:
                swordCollider.enabled = false;
                break;
        }
    }
}
