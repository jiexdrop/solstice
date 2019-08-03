﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [Header("Monster Prefabs")]
    public GameObject slimePrefab;

    public Dictionary<int, Room> rooms;
    public GameObject monstersParent;
    public Monster[] monsters = new Monster[GameManager.MAX_MONSTERS];
    public List<int> monstersToRemove = new List<int>();

    private Server server;
    private Client client;

    public void ClearMonsters()
    {
        for (int i = 0; i < monsters.Length; i++)
        {
            if (monsters[i] != null)
            {
                Destroy(monsters[i].gameObject);
                monsters[i] = null;
            }
        }
        Destroy(monstersParent);
    }

    public void SpawnMonsters(int roomId, int seed)
    {
        Room room = rooms[roomId];
        //Debug.Log(string.Format("Spawn monsters at room{0} key{1} with seed{2}", room, roomId, seed));
        Random.InitState(seed);
        ClearMonsters();
        monstersParent = new GameObject(string.Format("Room {0} monsters", roomId));

        for (int i = 0; i < 10; i++)
        {
            int rangeX = Random.Range(-(room.width - (room.width / 4)), (room.width - (room.width / 4))) / 2;
            int rangeY = Random.Range(-(room.height - (room.height / 4)), (room.height - (room.height / 4))) / 2;

            monsters[i] = Instantiate(slimePrefab, new Vector3Int(room.x + rangeX, room.y + rangeY, 0), Quaternion.identity, monstersParent.transform).GetComponent<Monster>();

            // Ignore collisions between players and ennemies
            if (server != null)
            {
                for (int j = 0; j < server.nbOfPlayers; j++)
                {
                    Physics2D.IgnoreCollision(monsters[i].GetComponent<Collider2D>(), server.players[j].GetComponent<Collider2D>());
                }
                monsters[i].GetComponent<Monster>().isServer = true;
                monsters[i].GetComponent<Monster>().players = server.players;
                monsters[i].GetComponent<Monster>().nbOfPlayers = server.nbOfPlayers;
            }
            if (client != null)
            {
                for (int j = 0; j < client.nbOfPlayers; j++)
                {
                    Physics2D.IgnoreCollision(monsters[i].GetComponent<Collider2D>(), client.players[j].GetComponent<Collider2D>());
                }
                monsters[i].GetComponent<Monster>().players = client.players;
                monsters[i].GetComponent<Monster>().nbOfPlayers = client.nbOfPlayers;
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
                switch (room.Value.type)
                {
                    case Room.Type.MONSTER:
                        if (room.Value.playerEntered && !room.Value.killedMonsters)
                        {
                            if (server != null)
                            {
                                server.dungeonGeneration.CloseRoom(room.Key);
                            }
                            if(client != null)
                            {
                                client.dungeonGeneration.CloseRoom(room.Key);
                            }
                            bool allMonstersDied = true;
                            for(int i = 0; i < monsters.Length; i++)
                            {
                                //Debug.Log(monsters[i]);
                                if (monsters[i] != null) // If one monster is alive
                                {
                                    allMonstersDied = false; // Then they haven't all died
                                }
                            }

                            // If all monsters died open room
                            if (allMonstersDied)
                            {
                                room.Value.killedMonsters = true;
                                if (server != null)
                                {
                                    server.dungeonGeneration.OpenRoom(room.Key);
                                }
                                if (client != null)
                                {
                                    client.dungeonGeneration.OpenRoom(room.Key);
                                }
                            }
                        }
                        break;
                }
                if (room.Value.inside && !room.Value.spawnedMonsters)
                {
                    switch (room.Value.type)
                    {
                        case Room.Type.MONSTER:
                            int seed = Random.Range(0, Int32.MaxValue);
                            SpawnMonsters(room.Key, seed);
                            if (server != null)
                            {
                                server.ShareSpawnMonsters(room.Key, 0, seed, true); // 0 server playerId
                            }
                            if (client != null)
                            {
                                client.SpawnMonsters(room.Key, seed, true);
                            }

                            break;
                    }
                }

            }
        }

        // Remove monsters if toRemove set
        for (int i = 0; i< monsters.Length; i++)
        {
            if (monsters[i] != null && monsters[i].toRemove)
            {
                Destroy(monsters[i].gameObject);
                Destroy(monsters[i]);
                monsters[i] = null;
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
