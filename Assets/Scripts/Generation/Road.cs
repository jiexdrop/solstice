using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

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