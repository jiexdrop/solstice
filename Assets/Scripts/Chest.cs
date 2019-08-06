using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public Sprite openSprite;
    public Sprite closedSprite;

    public GameObject pickablePrefab;

    public bool opened;

    public Room room;

    public Dictionary<Room, GameObject> pickables;

    public Server server;
    public Client client;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    public void OpenChest()
    {
        if (!opened)
        {
            GetComponent<SpriteRenderer>().sprite = openSprite;
            pickables[room] = Instantiate(pickablePrefab, transform.position, Quaternion.identity);
            pickables[room].GetComponent<Pickable>().server = server;
            pickables[room].GetComponent<Pickable>().client = client;
            opened = true;
        }
    }
}
