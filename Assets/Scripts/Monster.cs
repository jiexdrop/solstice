using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public int health = 3;

    private float elapsed;

    private Vector3 randomMovement;

    public bool isServer;
    public bool toRemove;

    public Dictionary<int, Monster> monsters;

    // Start is called before the first frame update
    void Start()
    {
        randomMovement = Random.insideUnitCircle * 4 * Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            elapsed += Time.deltaTime;
            if (elapsed > 5)
            { // every random second choose another direction
                elapsed = 0;
                randomMovement = Random.insideUnitCircle * 4 * Time.deltaTime;
            }

            GetComponent<Rigidbody2D>().MovePosition(transform.position + randomMovement);
        }

        if (health <= 0)
        {
            toRemove = true;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Projectile")
        {
            Debug.Log("Projectile");
            health--;
        }
    }
}
