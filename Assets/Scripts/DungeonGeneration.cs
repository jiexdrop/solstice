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

    // Tilemap 0 = Dark
    // Tilemap 1 = Wall
    // Tilemap 2 = Ground
    public TileBase[] tiles;

    public void Generate(int seed)
    {
        Debug.Log("Generating Dungeon with seed: " + seed);

        Random.InitState(seed);


        Room room = new Room(0, 0, 10, 10);
        //Room room = new Room(0, 0, 30, 20); // ?
        room.openings[0] = true;
        room.openings[1] = true;
        room.openings[2] = true;
        room.openings[3] = true;


        int limit = 4;
        GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, room, limit);

    }

    private void GenerateRecursively(Tilemap backgroundTilemap, Tilemap wallsTilemap, TileBase[] tiles, Room room, int limit)
    {
        room.Generate(backgroundTilemap, wallsTilemap, tiles);
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
                                Room nextRoom = new Room(road.x, road.y + room.height, 10, 10);
                                nextRoom.openings[(int)Opening.BOTTOM] = true;
                                if (backgroundTilemap.GetTile(new Vector3Int(nextRoom.x, nextRoom.y, 0)) == null)
                                {
                                    road.Generate(backgroundTilemap, wallsTilemap, tiles);
                                    GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, nextRoom, --limit);
                                }
                            }
                            break;
                        case (int)Opening.BOTTOM:
                            {
                                Road road = new Road(room.x, room.y - room.height, 5, room.height);
                                Room nextRoom = new Room(road.x, road.y - room.height, 10, 10);
                                nextRoom.openings[(int)Opening.TOP] = true;
                                if (backgroundTilemap.GetTile(new Vector3Int(nextRoom.x, nextRoom.y, 0)) == null)
                                {
                                    road.Generate(backgroundTilemap, wallsTilemap, tiles);
                                    GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, nextRoom, --limit);
                                }
                            }
                            break;
                        case (int)Opening.LEFT:
                            {
                                Road road = new Road(room.x - room.width, room.y, room.width, 5);
                                road.horizontal = true;
                                Room nextRoom = new Room(road.x - room.width, road.y, 10, 10);
                                nextRoom.openings[(int)Opening.RIGHT] = true;
                                if (backgroundTilemap.GetTile(new Vector3Int(nextRoom.x, nextRoom.y, 0)) == null)
                                {
                                    road.Generate(backgroundTilemap, wallsTilemap, tiles);
                                    GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, nextRoom, --limit);
                                }
                            }
                            break;
                        case (int)Opening.RIGHT:
                            {
                                Road road = new Road(room.x + room.width, room.y, room.width, 5);
                                road.horizontal = true;
                                Room nextRoom = new Room(road.x + room.width, road.y, 10, 10);
                                nextRoom.openings[(int)Opening.LEFT] = true;
                                if (backgroundTilemap.GetTile(new Vector3Int(nextRoom.x, nextRoom.y, 0)) == null)
                                {
                                    road.Generate(backgroundTilemap, wallsTilemap, tiles);
                                    GenerateRecursively(backgroundTilemap, wallsTilemap, tiles, nextRoom, --limit);
                                }
                            }
                            break;

                    }
                }
            }
        }
        // Close the room if no road 
        for(int i = 0; i < room.openings.Length; i++)
        {
            if(backgroundTilemap.GetTile(new Vector3Int(room.x, room.y + room.height, 0)) == null)
            {
                room.openings[(int)Opening.TOP] = false;
            }

            if (backgroundTilemap.GetTile(new Vector3Int(room.x, room.y - room.height, 0)) == null)
            {
                room.openings[(int)Opening.BOTTOM] = false;
            }

            if (backgroundTilemap.GetTile(new Vector3Int(room.x + room.width, room.y, 0)) == null)
            {
                room.openings[(int)Opening.RIGHT] = false;
            }

            if (backgroundTilemap.GetTile(new Vector3Int(room.x - room.width, room.y, 0)) == null)
            {
                room.openings[(int)Opening.LEFT] = false;
            }
        }
        room.Generate(backgroundTilemap, wallsTilemap, tiles);
    }


    internal void Clear()
    {
        backgroundTilemap.ClearAllTiles();
        wallsTilemap.ClearAllTiles();
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
                wallsTilemap.SetTile(new Vector3Int(-width / 2 + x, i, 0), tiles[7]);
            }
        }

        private void RightWall(Tilemap wallsTilemap, TileBase[] tiles)
        {
            for (int i = -height / 2 + 1 + y; i < height / 2 + y; i++)
            {
                wallsTilemap.SetTile(new Vector3Int(width / 2 + x, i, 0), tiles[7]);
            }
        }

        private void BottomWall(Tilemap wallsTilemap, TileBase[] tiles)
        {
            for (int i = -width / 2 + x + 1; i < width / 2 + x; i++)
            {
                wallsTilemap.SetTile(new Vector3Int(i, height / 2 + y, 0), tiles[7]);
            }
        }

        private void TopWall(Tilemap wallsTilemap, TileBase[] tiles)
        {
            for (int i = -width / 2 + x + 1; i < width / 2 + x; i++)
            {
                wallsTilemap.SetTile(new Vector3Int(i, -height / 2 + y, 0), tiles[7]);
            }
        }

        private void Ground(Tilemap backgroundTilemap, TileBase[] tiles)
        {
            for (int i = -width / 2 + x + 1; i < width / 2 + x; i++)
            {
                for (int j = -height / 2 + y + 1; j < height / 2 + y; j++)
                {
                    backgroundTilemap.SetTile(new Vector3Int(i, j, 0), tiles[2]);
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

            /*bool hasAtLeastOneOpening = false;
            for(int i = 0; i < openings.Length; i++)
            {
                if (openings[i])
                {
                    hasAtLeastOneOpening = true;
                }
            }
 
            if (!hasAtLeastOneOpening)
            {
                int makeOpening = Random.Range(0, openings.Length);
                openings[makeOpening] = true;
            }*/

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
                    backgroundTilemap.SetTile(new Vector3Int(i, j, 0), tiles[2]);
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
    }
}
