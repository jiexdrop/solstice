using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class DungeonGeneration : MonoBehaviour
{
    public Tilemap backgroundTilemap;
    public Tilemap wallsTilemap;
    public Tilemap foregroundTilemap;

    [Header("Prefabs")]
    public GameObject portalPrefab;
    public GameObject chestPrefab;

    public GameObject portal;

    public Dictionary<Room, GameObject> chests;

    // Tilemap 0 = Dark
    // Tilemap 1 = Wall
    // Tilemap 2 = DarkWall
    // Tilemap 3 = Ground
    // Tilemap 4 = Grass
    public TileBase[] tiles;

    public Dictionary<int, Room> rooms = new Dictionary<int, Room>();
    public List<Road> roads = new List<Road>();
    public Room lastRoom;
    public int maxDepth;

    private Server server;
    private Client client;

    private int seed;

    public void SetServer(Server server)
    {
        this.server = server;
    }

    public void SetClient(Client client)
    {
        this.client = client;
    }

    public void Generate(int seed)
    {
        Debug.Log("Generating Dungeon with seed: " + seed);

        maxDepth = 0;

        this.seed = seed;
        Random.InitState(seed);

        int randomWidth = Random.Range(5, 15) * 2;
        int randomHeight = Random.Range(5, 15) * 2;
        Room room = new Room(0, 0, randomWidth, randomHeight);
        room.type = Room.Type.ENTRY;

        bool hasAtLeastOneOpening = false;
        for (int i = 0; i < room.openings.Length; i++)
        {
            if (room.openings[i])
            {
                hasAtLeastOneOpening = true;
            }
        }

        if (!hasAtLeastOneOpening)
        {
            int makeOpening = Random.Range(0, room.openings.Length);
            room.openings[makeOpening] = true;
        }

        chests = new Dictionary<Room, GameObject>();
        int limit = 2;
        GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, room, chests, seed, limit, 0);

        if (chests.ContainsKey(lastRoom)) Destroy(chests[lastRoom]);
        lastRoom.type = Room.Type.PORTAL;
        portal = Instantiate(portalPrefab, new Vector3(lastRoom.x + 0.5f, lastRoom.y + 0.5f, 0), Quaternion.identity);
        portal.GetComponent<Portal>().SetServer(server);
        portal.GetComponent<Portal>().SetClient(client);

    }

    private void GenerateRecursively(Tilemap backgroundTilemap, Tilemap wallsTilemap, TileBase[] tiles, Room room, Dictionary<Room, GameObject> chests, int seed, int limit, int depth)
    {
        room.Generate(backgroundTilemap, wallsTilemap, tiles, seed);
        switch (room.type)
        {
            case Room.Type.CHEST:
                chests[room] = Instantiate(chestPrefab, new Vector3(room.x + 0.5f, room.y + 0.5f, 0), Quaternion.identity);
                break;
        }

        rooms[rooms.Count] = room;
        if (depth > maxDepth)
        {
            maxDepth = depth;
            lastRoom = room;
        }
        if (limit > 0)
        {
            for (int i = 0; i < room.openings.Length; i++)
            {
                if (room.openings[i])
                {
                    switch (i)
                    {
                        case (int)Opening.TOP:
                            {
                                Road road = new Road(room.x, room.y + room.height, 5, room.height);
                                int randomWidth = Random.Range(5, 15) * 2;
                                int randomHeight = Random.Range(5, 15) * 2;
                                Room nextRoom = new Room(road.x, road.y + (room.height - ((room.height - randomHeight) / 2)), randomWidth, randomHeight);
                                // Randomly generate chests
                                if (Random.Range(0, 2) == 0)
                                {
                                    nextRoom.type = Room.Type.MONSTER;
                                }
                                else
                                {
                                    nextRoom.type = Room.Type.CHEST;
                                }
                                nextRoom.openings[(int)Opening.BOTTOM] = true;
                                if (IsSpaceForRoom(backgroundTilemap, wallsTilemap, nextRoom, 2))
                                {
                                    if (IsSpaceForRoad(backgroundTilemap, wallsTilemap, road))
                                    {
                                        road.Generate(backgroundTilemap, wallsTilemap, tiles);
                                        roads.Add(road);
                                        GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, nextRoom, chests, seed, --limit, ++depth);
                                    }
                                }
                            }
                            break;
                        case (int)Opening.BOTTOM:
                            {
                                Road road = new Road(room.x, room.y - room.height, 5, room.height);
                                int randomWidth = Random.Range(5, 15) * 2;
                                int randomHeight = Random.Range(5, 15) * 2;
                                Room nextRoom = new Room(road.x, road.y - (room.height - ((room.height - randomHeight) / 2)), randomWidth, randomHeight);
                                // Randomly generate chests
                                if (Random.Range(0, 2) == 0)
                                {
                                    nextRoom.type = Room.Type.MONSTER;
                                }
                                else
                                {
                                    nextRoom.type = Room.Type.CHEST;
                                }
                                nextRoom.openings[(int)Opening.TOP] = true;
                                if (IsSpaceForRoom(backgroundTilemap, wallsTilemap, nextRoom, 2))
                                {
                                    if (IsSpaceForRoad(backgroundTilemap, wallsTilemap, road))
                                    {
                                        road.Generate(backgroundTilemap, wallsTilemap, tiles);
                                        roads.Add(road);
                                        GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, nextRoom, chests, seed, --limit, ++depth);
                                    }
                                }
                            }
                            break;
                        case (int)Opening.LEFT:
                            {
                                Road road = new Road(room.x - room.width, room.y, room.width, 5);
                                int randomWidth = Random.Range(5, 15) * 2;
                                int randomHeight = Random.Range(5, 15) * 2;
                                road.horizontal = true;
                                Room nextRoom = new Room(road.x - (room.width - ((room.width - randomWidth) / 2)), road.y, randomWidth, randomHeight);
                                // Randomly generate chests
                                if (Random.Range(0, 2) == 0)
                                {
                                    nextRoom.type = Room.Type.MONSTER;
                                }
                                else
                                {
                                    nextRoom.type = Room.Type.CHEST;
                                }
                                nextRoom.openings[(int)Opening.RIGHT] = true;
                                if (IsSpaceForRoom(backgroundTilemap, wallsTilemap, nextRoom, 2))
                                {
                                    if (IsSpaceForRoad(backgroundTilemap, wallsTilemap, road))
                                    {
                                        road.Generate(backgroundTilemap, wallsTilemap, tiles);
                                        roads.Add(road);
                                        GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, nextRoom, chests, seed, --limit, ++depth);
                                    }
                                }
                            }
                            break;
                        case (int)Opening.RIGHT:
                            {
                                Road road = new Road(room.x + room.width, room.y, room.width, 5);
                                int randomWidth = Random.Range(5, 15) * 2;
                                int randomHeight = Random.Range(5, 15) * 2;
                                road.horizontal = true;
                                Room nextRoom = new Room(road.x + (room.width - ((room.width - randomWidth) / 2)), road.y, randomWidth, randomHeight);
                                // Randomly generate chests
                                if (Random.Range(0, 2) == 0)
                                {
                                    nextRoom.type = Room.Type.MONSTER;
                                }
                                else
                                {
                                    nextRoom.type = Room.Type.CHEST;
                                }
                                nextRoom.openings[(int)Opening.LEFT] = true;
                                if (IsSpaceForRoom(backgroundTilemap, wallsTilemap, nextRoom, 2))
                                {
                                    if (IsSpaceForRoad(backgroundTilemap, wallsTilemap, road))
                                    {
                                        road.Generate(backgroundTilemap, wallsTilemap, tiles);
                                        roads.Add(road);
                                        GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, nextRoom, chests, seed, --limit, ++depth);
                                    }
                                }
                            }
                            break;

                    }
                }
            }
        }
        // Close the room if no road 
        for (int i = 0; i < room.openings.Length; i++)
        {
            if (backgroundTilemap.GetTile(new Vector3Int(room.x, room.y + room.height / 2 + 1, 0)) == null)
            {
                room.openings[(int)Opening.TOP] = false;
            }

            if (backgroundTilemap.GetTile(new Vector3Int(room.x, room.y - room.height / 2 - 1, 0)) == null)
            {
                room.openings[(int)Opening.BOTTOM] = false;
            }

            if (backgroundTilemap.GetTile(new Vector3Int(room.x + room.width / 2 + 1, room.y, 0)) == null)
            {
                room.openings[(int)Opening.RIGHT] = false;
            }

            if (backgroundTilemap.GetTile(new Vector3Int(room.x - room.width / 2 - 1, room.y, 0)) == null)
            {
                room.openings[(int)Opening.LEFT] = false;
            }

        }
        room.Generate(backgroundTilemap, wallsTilemap, tiles, seed);
    }

    internal void CloseRoom(int key)
    {
        rooms[key].Close(backgroundTilemap, wallsTilemap, tiles, seed);
    }

    internal void OpenRoom(int key)
    {
        rooms[key].Generate(backgroundTilemap, wallsTilemap, tiles, seed);
    }

    internal void HighlightRoom(Player player)
    {
        foreach (Room room in rooms.Values)
        {
            bool playerIsInside = room.PlayerIsInside(player);
            room.playerEntered = room.PlayerEntered(player);

            if (playerIsInside)
            {
                if (!room.inside)
                {
                    room.entering = true;
                    room.inside = true;
                }
            }

            if (room.entering)
            {
                room.Highlight(backgroundTilemap, wallsTilemap, tiles, seed);
                room.entering = false;
            }

            if (room.inside && !playerIsInside)
            {
                room.inside = false;
                room.Generate(backgroundTilemap, wallsTilemap, tiles, seed);
            }
        }
    }

    private bool IsSpaceForRoad(Tilemap backgroundTilemap, Tilemap wallsTilemap, Road road)
    {
        for (int i = -road.width / 2 + road.x + 1; i < road.width / 2 + road.x; i++)
        {
            for (int j = -road.height / 2 + road.y + 1; j < road.height / 2 + road.y; j++)
            {
                if (backgroundTilemap.GetTile(new Vector3Int(i, j, 0)) != null)
                {
                    return false;
                }
                if (wallsTilemap.GetTile(new Vector3Int(i, j, 0)) != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool IsSpaceForRoom(Tilemap backgroundTilemap, Tilemap wallsTilemap, Room room, int margin)
    {
        for (int i = -room.width / 2 + room.x - margin; i < room.width / 2 + room.x + margin; i++)
        {
            for (int j = -room.height / 2 + room.y - margin; j < room.height / 2 + room.y + margin; j++)
            {
                if (backgroundTilemap.GetTile(new Vector3Int(i, j, 0)) != null)
                {
                    return false;
                }
                if (wallsTilemap.GetTile(new Vector3Int(i, j, 0)) != null)
                {
                    return false;
                }
            }
        }
        return true;
    }


    public Vector2 GetPositionByPlayerDirection(int playerId, int roomId, Vector2 direction)
    {
        int entranceId = playerId;
        if (entranceId > 2)
        {
            entranceId = Random.Range(0, 3);
        }
        if (direction.x == -1)
        {
            return rooms[roomId].rightEntrances[entranceId];
        }
        if (direction.x == 1)
        {
            return rooms[roomId].leftEntrances[entranceId];
        }
        if (direction.y == 1)
        {
            return rooms[roomId].bottomEntrances[entranceId];
        }
        if (direction.y == -1)
        {
            return rooms[roomId].topEntrances[entranceId];
        }
        return new Vector2(rooms[roomId].x, rooms[roomId].y);
    }

    internal void Clear()
    {
        rooms.Clear();
        roads.Clear();
        backgroundTilemap.ClearAllTiles();
        wallsTilemap.ClearAllTiles();

        foreach (GameObject chest in chests.Values)
        {
            Destroy(chest);
        }
        chests.Clear();
        Destroy(portal);
    }


}
