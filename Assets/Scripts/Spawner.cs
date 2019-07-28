using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [Header("Monster Prefabs")]
    public GameObject slimePrefab;

    public Dictionary<int, Room> rooms;
    public GameObject monsters;

    private Server server;
    private Client client;

    public void ClearMonsters()
    {
        Destroy(monsters);
    }

    public void SpawnMonsters(int roomId, int seed)
    {
        Room room = rooms[roomId];
        //Debug.Log(string.Format("Spawn monsters at room{0} key{1} with seed{2}", room, roomId, seed));
        Random.InitState(seed);
        ClearMonsters();
        monsters = new GameObject(string.Format("Room {0} Monsters", roomId));
        for (int i = 0; i < 10; i++)
        {
            int rangeX = Random.Range(-(room.width - (room.width / 4)), (room.width - (room.width / 4))) / 2;
            int rangeY = Random.Range(-(room.height - (room.height / 4)), (room.height - (room.height / 4))) / 2;
            GameObject slime = Instantiate(slimePrefab, new Vector3Int(room.x + rangeX, room.y + rangeY, 0), Quaternion.identity, monsters.transform);
            // Ignore collisions between players and ennemies
            if (server != null)
            {
                for(int j = 0; j<server.nbOfPlayers; j++)
                {
                    Physics2D.IgnoreCollision(slime.GetComponent<Collider2D>(), server.players[j].GetComponent<Collider2D>());
                }
                slime.GetComponent<Monster>().isServer = true;
            } 
            if(client != null)
            {
                for (int j = 0; j < client.nbOfPlayers; j++)
                {
                    Physics2D.IgnoreCollision(slime.GetComponent<Collider2D>(), client.players[j].GetComponent<Collider2D>());
                }
            }
        }

        room.spawnedMonsters = true;
        room.inside = true; // TODO Remove me
    }

    void Update()
    {
        if (rooms != null) // Before setting rooms dont do anything
        {
            foreach (KeyValuePair<int, Room> room in rooms)
            {
                if (room.Value.inside && !room.Value.spawnedMonsters)
                {
                    switch (room.Value.type)
                    {
                        case Room.Type.MONSTER:
                            int seed = Random.Range(0, Int32.MaxValue);
                            SpawnMonsters(room.Key, seed);
                            if (server != null)
                            {
                                server.ShareSpawnMonsters(room.Key, 0, seed); // 0 server playerId
                            }
                            if (client != null)
                            {
                                client.SpawnMonsters(room.Key, seed);
                            }

                            break;
                    }
                }

            }
        }

    }

    public void SetServer(Server server)
    {
        this.server = server;
    }

    public void SetClient(Client client)
    {
        this.client = client;
    }
}
