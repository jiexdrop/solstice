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

    public GameObject portal;

    // Tilemap 0 = Dark
    // Tilemap 1 = Wall
    // Tilemap 2 = DarkWall
    // Tilemap 3 = Ground
    // Tilemap 4 = Grass
    public TileBase[] tiles;

    public List<Room> rooms = new List<Room>();
    public List<Road> roads = new List<Road>();
    public Room lastRoom;
    public int maxDepth;

    private Server server;
    private Client client;

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

        Random.InitState(seed);

        int randomWidth = Random.Range(5, 15) * 2;
        int randomHeight = Random.Range(5, 15) * 2;
        Room room = new Room(0, 0, randomWidth, randomHeight);

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

        int limit = 2;
        GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, room, limit, 0);

        room.DrawEntrance(backgroundTilemap, tiles);

        portal = Instantiate(portalPrefab, new Vector3Int(lastRoom.x, lastRoom.y, 0), Quaternion.identity);
        portal.GetComponent<Portal>().SetServer(server);
        portal.GetComponent<Portal>().SetClient(client);

    }

    private void GenerateRecursively(Tilemap backgroundTilemap, Tilemap wallsTilemap, TileBase[] tiles, Room room, int limit, int depth)
    {
        room.Generate(backgroundTilemap, wallsTilemap, tiles);
        rooms.Add(room);
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
                                nextRoom.openings[(int)Opening.BOTTOM] = true;
                                if (IsSpaceForRoom(backgroundTilemap, wallsTilemap, nextRoom, 2))
                                {
                                    if (IsSpaceForRoad(backgroundTilemap, wallsTilemap, road))
                                    {
                                        road.Generate(backgroundTilemap, wallsTilemap, tiles);
                                        roads.Add(road);
                                        GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, nextRoom, --limit, ++depth);
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
                                nextRoom.openings[(int)Opening.TOP] = true;
                                if (IsSpaceForRoom(backgroundTilemap, wallsTilemap, nextRoom, 2))
                                {
                                    if (IsSpaceForRoad(backgroundTilemap, wallsTilemap, road))
                                    {
                                        road.Generate(backgroundTilemap, wallsTilemap, tiles);
                                        roads.Add(road);
                                        GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, nextRoom, --limit, ++depth);
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
                                nextRoom.openings[(int)Opening.RIGHT] = true;
                                if (IsSpaceForRoom(backgroundTilemap, wallsTilemap, nextRoom, 2))
                                {
                                    if (IsSpaceForRoad(backgroundTilemap, wallsTilemap, road))
                                    {
                                        road.Generate(backgroundTilemap, wallsTilemap, tiles);
                                        roads.Add(road);
                                        GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, nextRoom, --limit, ++depth);
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
                                nextRoom.openings[(int)Opening.LEFT] = true;
                                if (IsSpaceForRoom(backgroundTilemap, wallsTilemap, nextRoom, 2))
                                {
                                    if (IsSpaceForRoad(backgroundTilemap, wallsTilemap, road))
                                    {
                                        road.Generate(backgroundTilemap, wallsTilemap, tiles);
                                        roads.Add(road);
                                        GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, nextRoom, --limit, ++depth);
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
        room.Generate(backgroundTilemap, wallsTilemap, tiles);
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

    internal void Clear()
    {
        backgroundTilemap.ClearAllTiles();
        wallsTilemap.ClearAllTiles();

        Destroy(portal);
    }

    public enum Opening
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,

        COUNT
    }

    public class Road
    {

        public bool horizontal;
        public int x;
        public int y;
        public int width;
        public int height;

        public Road(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public void Generate(Tilemap backgroundTilemap, Tilemap wallsTilemap, TileBase[] tiles)
        {
            if (horizontal)
            {
                TopWall(wallsTilemap, tiles);
                BottomWall(wallsTilemap, tiles);
                Ground(backgroundTilemap, tiles);
            }
            else
            {
                LeftWall(wallsTilemap, tiles);
                RightWall(wallsTilemap, tiles);
                Ground(backgroundTilemap, tiles);
            }

        }

        private void LeftWall(Tilemap wallsTilemap, TileBase[] tiles)
        {
            for (int i = -height / 2 + y + 1; i < height / 2 + y; i++)
            {
                wallsTilemap.SetTile(new Vector3Int(-width / 2 + x, i, 0), tiles[2]);
            }
        }

        private void RightWall(Tilemap wallsTilemap, TileBase[] tiles)
        {
            for (int i = -height / 2 + 1 + y; i < height / 2 + y; i++)
            {
                wallsTilemap.SetTile(new Vector3Int(width / 2 + x, i, 0), tiles[2]);
            }
        }

        private void BottomWall(Tilemap wallsTilemap, TileBase[] tiles)
        {
            for (int i = -width / 2 + x + 1; i < width / 2 + x; i++)
            {
                wallsTilemap.SetTile(new Vector3Int(i, height / 2 + y, 0), tiles[2]);
            }
        }

        private void TopWall(Tilemap wallsTilemap, TileBase[] tiles)
        {
            for (int i = -width / 2 + x + 1; i < width / 2 + x; i++)
            {
                wallsTilemap.SetTile(new Vector3Int(i, -height / 2 + y, 0), tiles[2]);
            }
        }

        private void Ground(Tilemap backgroundTilemap, TileBase[] tiles)
        {
            for (int i = -width / 2 + x + 1; i < width / 2 + x; i++)
            {
                for (int j = -height / 2 + y + 1; j < height / 2 + y; j++)
                {
                    if (Random.Range(0, 16) > 2)
                    {
                        backgroundTilemap.SetTile(new Vector3Int(i, j, 0), tiles[3]);
                    }
                    else
                    {
                        backgroundTilemap.SetTile(new Vector3Int(i, j, 0), tiles[4]);
                    }
                }
            }
        }
    }

    public class Room
    {
        public bool[] openings = new bool[(int)Opening.COUNT];
        public int x;
        public int y;
        public int width;
        public int height;

        public Room(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            openings[(int)Opening.TOP] = (Random.Range(0, 2) == 1) ? true : false;
            openings[(int)Opening.BOTTOM] = (Random.Range(0, 2) == 1) ? true : false;
            openings[(int)Opening.LEFT] = (Random.Range(0, 2) == 1) ? true : false;
            openings[(int)Opening.RIGHT] = (Random.Range(0, 2) == 1) ? true : false;

        }

        public void Generate(Tilemap backgroundTilemap, Tilemap wallsTilemap, TileBase[] tiles)
        {
            TopWall(wallsTilemap, tiles, openings[(int)Opening.TOP]);
            BottomWall(wallsTilemap, tiles, openings[(int)Opening.BOTTOM]);
            LeftWall(wallsTilemap, tiles, openings[(int)Opening.LEFT]);
            RightWall(wallsTilemap, tiles, openings[(int)Opening.RIGHT]);

            Ground(backgroundTilemap, tiles);
        }

        private void Ground(Tilemap backgroundTilemap, TileBase[] tiles)
        {
            for (int i = -width / 2 + x; i <= width / 2 + x; i++)
            {
                for (int j = -height / 2 + y; j <= height / 2 + y; j++)
                {
                    if (Random.Range(0, 16) > 2)
                    {
                        backgroundTilemap.SetTile(new Vector3Int(i, j, 0), tiles[3]);
                    }
                    else
                    {
                        backgroundTilemap.SetTile(new Vector3Int(i, j, 0), tiles[4]);
                    }
                }
            }
        }

        public void TopWall(Tilemap tilemap, TileBase[] tiles, bool open)
        {
            for (int i = -width / 2 + x; i < width / 2 + x; i++)
            {
                tilemap.SetTile(new Vector3Int(i, height / 2 + y, 0), tiles[1]);
                if (open && i >= x - 1 && i <= x + 1)
                {
                    tilemap.SetTile(new Vector3Int(i, height / 2 + y, 0), null);
                }
            }
        }

        public void LeftWall(Tilemap tilemap, TileBase[] tiles, bool open)
        {
            for (int i = -height / 2 + y; i < height / 2 + y; i++)
            {
                tilemap.SetTile(new Vector3Int(-width / 2 + x, i, 0), tiles[1]);
                if (open && i >= y - 1 && i <= y + 1)
                {
                    tilemap.SetTile(new Vector3Int(-width / 2 + x, i, 0), null);
                }
            }
        }

        public void RightWall(Tilemap tilemap, TileBase[] tiles, bool open)
        {
            for (int i = -height / 2 + y; i <= height / 2 + y; i++)
            {
                tilemap.SetTile(new Vector3Int(width / 2 + x, i, 0), tiles[1]);
                if (open && i >= y - 1 && i <= y + 1)
                {
                    tilemap.SetTile(new Vector3Int(width / 2 + x, i, 0), null);
                }
            }
        }

        public void BottomWall(Tilemap tilemap, TileBase[] tiles, bool open)
        {
            for (int i = -width / 2 + x; i < width / 2 + x; i++)
            {
                tilemap.SetTile(new Vector3Int(i, -height / 2 + y, 0), tiles[1]);
                if (open && i >= x - 1 && i <= x + 1)
                {
                    tilemap.SetTile(new Vector3Int(i, -height / 2 + y, 0), null);
                }

            }
        }

        internal void DrawEntrance(Tilemap tilemap, TileBase[] tiles)
        {
            for (int i = x - 1; i < x + 1; i++)
            {
                for (int j = y - 1; j < y + 1; j++)
                {
                    tilemap.SetTile(new Vector3Int(i, j, 0), tiles[7]);
                }
            }
        }
    }
}
