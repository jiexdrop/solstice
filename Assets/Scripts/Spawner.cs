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
    public Dictionary<int, GameObject> monsters;

    private Server server;

    void Start()
    {
        monsters = new Dictionary<int, GameObject>();
    }

    public void ClearMonsters()
    {
        foreach (GameObject monster in monsters.Values)
        {
            Destroy(monster);
        }
        monsters.Clear();
    }

    public void SpawnMonsters(int key)
    {
        Room room = rooms[key];
        Debug.Log(string.Format("Spawn monsters at room{0} key{1}", room, key));
        for (int i = 0; i < 10; i++)
        {
            int rangeX = Random.Range(-(room.width - (room.width / 4)), (room.width - (room.width / 4))) / 2;
            int rangeY = Random.Range(-(room.height - (room.height / 4)), (room.height - (room.height / 4))) / 2;
            monsters[monsters.Count] = Instantiate(slimePrefab, new Vector3Int(room.x + rangeX, room.y + rangeY, 0), Quaternion.identity);
        }
        room.spawnedMonsters = true;
        room.inside = true; // TODO Remove me
    }

    void Update()
    {

        foreach (KeyValuePair<int, Room> room in rooms)
        {
            if (room.Value.inside && !room.Value.spawnedMonsters)
            {
                switch (room.Value.type)
                {
                    case Room.Type.MONSTER:
                        SpawnMonsters(room.Key);
                        if (server != null)
                        {
                            server.ShareSpawnMonsters(room.Key);
                        }
                        break;
                }
            }

            if (!room.Value.inside && room.Value.spawnedMonsters)
            {
                room.Value.spawnedMonsters = false;
                ClearMonsters();
            }
        }

    }

    public void SetServer(Server server)
    {
        this.server = server;
    }
}
