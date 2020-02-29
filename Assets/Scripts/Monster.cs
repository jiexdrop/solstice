using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterType
{
    SLIME,
    WITCH,
    OGRE
}

public class Monster : MonoBehaviour
{
    public int health = 3;

    private float elapsedMovement;
    private float elapsedShoot;

    private Vector3 randomMovement;

    public bool isServer;
    public bool toRemove;

    public GameObject projectilePrefab;

    public GameObject[] players = new GameObject[4];
    public int nbOfPlayers;

    private Renderer r;
    private float colorLasting;
    // Start is called before the first frame update
    void Start()
    {
        randomMovement = Random.insideUnitCircle * 4 * Time.deltaTime;
        r = GetComponentInChildren<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            elapsedMovement += Time.deltaTime;
            if (elapsedMovement > 5)
            { // every random second choose another direction
                elapsedMovement = 0;
                randomMovement = Random.insideUnitCircle * 4 * Time.deltaTime;
            }

            GetComponent<Rigidbody2D>().MovePosition(transform.position + randomMovement);
        }

        elapsedShoot += Time.deltaTime;
        if (elapsedShoot > Random.Range(1.5f, 3f))
        {
            elapsedShoot = 0;
            MonsterShoot();
        }

        if (health <= 0)
        {
            toRemove = true;
        }


        if(r.material.color.Equals(Color.red) && colorLasting <= 0)
        {
            r.material.color = Color.white;
        }

        if (colorLasting > 0)
        {
            colorLasting -= Time.deltaTime;
        }
    }

    private void MonsterShoot()
    {
        GameObject p = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Projectile projectile = p.GetComponent<Projectile>();
        projectile.duration = GameManager.SHOOT_DURATION * 2;
        projectile.speed = 5f;
        Quaternion rotation = Random.rotation;
        rotation.x = 0; rotation.y = 0;
        int playerId = Random.Range(0, nbOfPlayers);
        float maxDist = Vector3.Distance(players[0].transform.position, transform.position);
        // get nearest player
        for(int i = 0; i < nbOfPlayers; i++)
        {
            float dist = Vector3.Distance(players[i].transform.position, transform.position);
            if (dist < maxDist)
            {
                playerId = i;
                maxDist = dist;
            }
        }
        float angle = Mathf.Atan2(players[playerId].transform.position.y - transform.position.y, players[playerId].transform.position.x - transform.position.x);

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), p.GetComponent<Collider2D>());

        rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        projectile.transform.rotation = rotation;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerProjectile"))
        {
            health -= collision.GetComponent<Projectile>().damage;
            r.material.color = Color.red;
            colorLasting += Time.deltaTime * 2;
        }

        if (collision.gameObject.CompareTag("PlayerSword"))
        {
            health -= 3;
            r.material.color = Color.red;
            colorLasting += Time.deltaTime * 2;
        }
    }

}
