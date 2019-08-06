using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickable : MonoBehaviour
{
    public enum Type
    {
        POTION,
        WEAPON
    }

    private Vector2 upPosition;
    private bool displayed;

    public Server server;
    public Client client;

    public Type type;

    private float elapsed;

    private GameObject followPlayer;

    // Start is called before the first frame update
    void Start()
    {
        upPosition = transform.position;
        upPosition.y += 0.5f;
        this.type = Type.POTION;
    }

    // Update is called once per frame
    void Update()
    {
        if (!displayed)
        {
            transform.position = Vector3.Lerp(transform.position, upPosition, Time.deltaTime);
            if (Vector3.Distance(transform.position, upPosition) < 0.1)
            {
                displayed = true;
            }
        }

        if (displayed  && followPlayer != null)
        {
            transform.position = Vector3.Lerp(transform.position, followPlayer.transform.position, Time.deltaTime * 5);
            if(Vector3.Distance(transform.position, followPlayer.transform.position) < 0.1)
            {
                followPlayer.GetComponent<Player>().UsePickable(this);
                Destroy(this.gameObject);
                Destroy(this);
            }
        }

        elapsed += Time.deltaTime;

        if (elapsed > 0.5f)
        {
            // Check if player in radius
            if (server != null)
            {
                for(int i = 0; i< server.nbOfPlayers; i++)
                {
                    Vector3 playerPos = server.players[i].transform.position;
                    if (Vector3.Distance(playerPos, transform.position) < 2f)
                    {
                        followPlayer = server.players[i];
                    }
                }

            }
            if (client != null)
            {
                for (int i = 0; i < client.nbOfPlayers; i++)
                {
                    Vector3 playerPos = client.players[i].transform.position;
                    if (Vector3.Distance(playerPos, transform.position) < 2f)
                    {
                        followPlayer = client.players[i];
                    }
                }

            }
        }
    }
}
