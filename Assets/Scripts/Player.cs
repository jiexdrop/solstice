using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public GameObject center;
    public GameObject visor;

    public Animator animator;

    public float visorRotation;

    [Header("Health")]
    public Slider healthBar;
    public int health = 10;

    public bool isPlayed; // I'm currently being played or do my values come from the server

    public bool died;

    public Server server;
    public Client client;

    void Start()
    {
        center = transform.GetChild(0).gameObject;
        visor = center.transform.GetChild(0).gameObject;

        animator = GetComponent<Animator>();
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
    }
}
