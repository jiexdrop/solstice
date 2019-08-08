using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    Server server;
    Client client;

    public void SetServer(Server server)
    {
        this.server = server;
    }

    internal void SetClient(Client client)
    {
        this.client = client;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (server != null)
            {
                server.GoToNextRoom();
            }

            if (client != null)
            {
                client.GoToNextRoom();
            }
        }
    }
}
