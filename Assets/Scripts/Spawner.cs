using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [Header("Monster Prefabs")]
    public GameObject[] monsterPrefabs;

    public Dictionary<int, Room> rooms;
    public GameObject monstersParent;
    public Monster[] monsters = new Monster[GameManager.MAX_MONSTERS];
    public List<int> monstersToRemove = new List<int>();

    private Server server;
    private Client client;

    public List<Wave> waves;
    public int waveIndex;

    public int lastRoomKey;

    public int seed;

    [Serializable]
    public class Wave
    {
        public List<MonsterCount> monsterCounts;
    }

    [Serializable]
    public class MonsterCount
    {
        public MonsterType monsterType;
        public int count;
    }

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

    public void SpawnMonsters(int roomId, int seed, Wave wave)
    {
        Room room = rooms[roomId];
        //Debug.Log(string.Format("Spawn monsters at room{0} key{1} with seed{2}", room, roomId, seed));
        Random.InitState(seed);
        ClearMonsters();
        monstersParent = new GameObject(string.Format("Room {0} monsters", roomId));

        for (int k = 0; k < wave.monsterCounts.Count; k++)
        {
            for (int i = 0; i < wave.monsterCounts[k].count; i++)
            {
                int rangeX = Random.Range(-(room.width - (room.width / 4)), (room.width - (room.width / 4))) / 2;
                int rangeY = Random.Range(-(room.height - (room.height / 4)), (room.height - (room.height / 4))) / 2;

                monsters[i] = Instantiate(monsterPrefabs[(int)wave.monsterCounts[k].monsterType], new Vector3Int(room.x + rangeX, room.y + rangeY, 0), Quaternion.identity, monstersParent.transform).GetComponent<Monster>();

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
                if (room.Value.playerEntered && !room.Value.killedMonsters)
                {
                    switch (room.Value.type)
                    {
                        case Room.Type.MONSTER:
                            bool allMonstersDied = true;
                            for (int i = 0; i < monsters.Length; i++)
                            {
                                //Debug.Log(monsters[i]);
                                if (monsters[i] != null) // If one monster is alive
                                {
                                    allMonstersDied = false; // Then they haven't all died
                                }
                            }

                            // If all monsters died open room
                            if (allMonstersDied && waveIndex < waves.Count)
                            {
                                room.Value.spawnedMonsters = false;
                                waveIndex++;
                            }
                            if (allMonstersDied && waveIndex >= waves.Count)
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
                                room.Value.spawnedMonsters = true;
                            }
                            break;
                    }
                }



                if (room.Value.inside && !room.Value.spawnedMonsters)
                {
                    switch (room.Value.type)
                    {
                        case Room.Type.MONSTER:
                            //when we change room we reset spawner
                            if(room.Key != lastRoomKey)
                            {
                                waveIndex = 0;
                                lastRoomKey = room.Key;
                            }

                            SpawnMonsters(room.Key, seed, waves[waveIndex]);
                            
                            if (server != null)
                            {
                                server.ShareSpawnMonsters(room.Key, 0, seed, true); // 0 server playerId
                                server.dungeonGeneration.CloseRoom(room.Key);
                            }
                            if (client != null)
                            {
                                client.SpawnMonsters(room.Key, seed, true);
                                client.dungeonGeneration.CloseRoom(room.Key);
                            }

                            break;
                    }
                }

            }
        }

        // Remove monsters if toRemove set
        for (int i = 0; i < monsters.Length; i++)
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
