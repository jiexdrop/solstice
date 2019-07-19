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

        for(int i = -10; i < 10; i++)
        {
            for(int j = -10; j < 10; j++)
            {
                Room room = new Room(i * 10, j * 10, 10, 10);
                room.Generate(backgroundTilemap, wallsTilemap, tiles);
            }
        }

    }

    public enum Opening {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,
        
        COUNT
    }

    public class Room
    {
        public bool[] openings = new bool[(int)Opening.COUNT];
        int x;
        int y;
        int width;
        int height;

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
            for(int i = -width/2 + x; i <= width/2 + x; i++)
            {
                for(int j = -height/2 + y; j <= height/2 + y; j++)
                {
                    backgroundTilemap.SetTile(new Vector3Int(i, j, 0), tiles[2]);
                }
            }
        }

        public void TopWall(Tilemap tilemap, TileBase[] tiles, bool open)
        {
            for (int i = -width / 2 + x; i < width / 2 + x; i++)
            {
                tilemap.SetTile(new Vector3Int(i, height/2 + y, 0), tiles[1]);
                if (open && i >= x - 1 && i <= x + 1)
                {
                    tilemap.SetTile(new Vector3Int(i, height / 2 + y, 0), null);
                }
            }
        }

        public void LeftWall(Tilemap tilemap, TileBase[] tiles, bool open)
        {
            for (int i = -height/2 + y; i < height/2 + y; i++)
            {
                tilemap.SetTile(new Vector3Int(-width/2 + x, i, 0), tiles[1]);
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
                if(open && i >= y - 1 && i <= y + 1)
                {
                    tilemap.SetTile(new Vector3Int(width / 2 + x, i, 0), null);
                }
            }
        }

        public void BottomWall(Tilemap tilemap, TileBase[] tiles, bool open)
        {
            for (int i = -width/2 + x; i < width/2 + x; i++)
            {
                tilemap.SetTile(new Vector3Int(i, -height / 2 + y, 0), tiles[1]);
                if (open && i >= x -1 && i <= x + 1)
                {
                    tilemap.SetTile(new Vector3Int(i, -height / 2 + y, 0), null);
                } 
                
            }
        }
    }
}
