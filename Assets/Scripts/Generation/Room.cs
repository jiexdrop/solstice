using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class Room
{
    public enum Type
    {
        ENTRY,
        PORTAL,
        MONSTER
    }

    public bool[] openings = new bool[(int)Opening.COUNT];

    public Vector2[] topEntrances = new Vector2[3];
    public Vector2[] bottomEntrances = new Vector2[3];
    public Vector2[] leftEntrances = new Vector2[3];
    public Vector2[] rightEntrances = new Vector2[3];

    public int x;
    public int y;
    public int width;
    public int height;

    public bool entering;
    public bool inside;
    public bool spawnedMonsters;

    public Type type;

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

    public void Generate(Tilemap backgroundTilemap, Tilemap wallsTilemap, TileBase[] tiles, int seed)
    {
        Random.InitState(seed);

        Ground(backgroundTilemap, tiles);

        TopWall(wallsTilemap, tiles[1], openings[(int)Opening.TOP]);
        TopEntrances(backgroundTilemap, tiles[8], openings[(int)Opening.TOP]);
        BottomWall(wallsTilemap, tiles[1], openings[(int)Opening.BOTTOM]);
        BottomEntrances(backgroundTilemap, tiles[8], openings[(int)Opening.BOTTOM]);
        LeftWall(wallsTilemap, tiles[1], openings[(int)Opening.LEFT]);
        LeftEntrances(backgroundTilemap, tiles[8], openings[(int)Opening.LEFT]);
        RightWall(wallsTilemap, tiles[1], openings[(int)Opening.RIGHT]);
        RightEntrances(backgroundTilemap, tiles[8], openings[(int)Opening.RIGHT]);

        switch (type)
        {
            case Room.Type.ENTRY:
                DrawEntrance(backgroundTilemap, tiles);
                break;
        }
    }

    public void Close(Tilemap backgroundTilemap, Tilemap wallsTilemap, TileBase[] tiles, int seed)
    {
        Random.InitState(seed);

        TopWall(wallsTilemap, tiles[1], false);
        BottomWall(wallsTilemap, tiles[1], false);
        LeftWall(wallsTilemap, tiles[1], false);
        RightWall(wallsTilemap, tiles[1], false);

        Ground(backgroundTilemap, tiles);

        switch (type)
        {
            case Room.Type.ENTRY:
                DrawEntrance(backgroundTilemap, tiles);
                break;
        }
    }

    public void Highlight(Tilemap backgroundTilemap, Tilemap wallsTilemap, TileBase[] tiles, int seed)
    {
        Random.InitState(seed);

        Ground(backgroundTilemap, tiles);

        TopWall(wallsTilemap, tiles[10], openings[(int)Opening.TOP]);
        TopEntrances(backgroundTilemap, tiles[8], openings[(int)Opening.TOP]);
        BottomWall(wallsTilemap, tiles[10], openings[(int)Opening.BOTTOM]);
        BottomEntrances(backgroundTilemap, tiles[8], openings[(int)Opening.BOTTOM]);
        LeftWall(wallsTilemap, tiles[10], openings[(int)Opening.LEFT]);
        LeftEntrances(backgroundTilemap, tiles[8], openings[(int)Opening.LEFT]);
        RightWall(wallsTilemap, tiles[10], openings[(int)Opening.RIGHT]);
        RightEntrances(backgroundTilemap, tiles[8], openings[(int)Opening.RIGHT]);

        switch (type)
        {
            case Room.Type.ENTRY:
                DrawEntrance(backgroundTilemap, tiles);
                break;
        }
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

    public void TopWall(Tilemap tilemap, TileBase tile, bool open)
    {
        for (int i = -width / 2 + x; i < width / 2 + x; i++)
        {
            tilemap.SetTile(new Vector3Int(i, height / 2 + y, 0), tile);
            if (open && i >= x - 1 && i <= x + 1)
            {
                tilemap.SetTile(new Vector3Int(i, height / 2 + y, 0), null);
            }
        }
    }

    public void TopEntrances(Tilemap tilemap, TileBase tile, bool open)
    {
        int j = 0;
        for (int i = x - 1; i <= x + 1; i++)
        {
            tilemap.SetTile(new Vector3Int(i, height / 2 + y - 1, 0), tile);
            topEntrances[j] = new Vector2(i, height / 2 + y - 1);
            j++;
        }
    }

    public void LeftWall(Tilemap tilemap, TileBase tile, bool open)
    {
        for (int i = -height / 2 + y; i < height / 2 + y; i++)
        {
            tilemap.SetTile(new Vector3Int(-width / 2 + x, i, 0), tile);
            if (open && i >= y - 1 && i <= y + 1)
            {
                tilemap.SetTile(new Vector3Int(-width / 2 + x, i, 0), null);
            }
        }
    }

    public void LeftEntrances(Tilemap tilemap, TileBase tile, bool open)
    {
        int j = 0;
        for (int i = y - 1; i <= y + 1; i++)
        {
            tilemap.SetTile(new Vector3Int(-width / 2 + 1 + x, i, 0), tile);
            leftEntrances[j] = new Vector2(-width / 2 + 1 + x, i);
            j++;
        }
    }

    public void RightWall(Tilemap tilemap, TileBase tile, bool open)
    {
        for (int i = -height / 2 + y; i <= height / 2 + y; i++)
        {
            tilemap.SetTile(new Vector3Int(width / 2 + x, i, 0), tile);
            if (open && i >= y - 1 && i <= y + 1)
            {
                tilemap.SetTile(new Vector3Int(width / 2 + x, i, 0), null);
            }
        }
    }

    public void RightEntrances(Tilemap tilemap, TileBase tile, bool open)
    {
        int j = 0;
        for (int i = y - 1; i <= y + 1; i++)
        {
            tilemap.SetTile(new Vector3Int(width / 2 - 1 + x, i, 0), tile);
            rightEntrances[j] = new Vector2(width / 2 - 1 + x, i);
            j++;
        }
    }

    public void BottomWall(Tilemap tilemap, TileBase tile, bool open)
    {
        for (int i = -width / 2 + x; i < width / 2 + x; i++)
        {
            tilemap.SetTile(new Vector3Int(i, -height / 2 + y, 0), tile);
            if (open && i >= x - 1 && i <= x + 1)
            {
                tilemap.SetTile(new Vector3Int(i, -height / 2 + y, 0), null);
            }

        }
    }

    public void BottomEntrances(Tilemap tilemap, TileBase tile, bool open)
    {
        int j = 0;
        for (int i = x - 1; i <= x + 1; i++)
        {
            tilemap.SetTile(new Vector3Int(i, -height / 2 + y + 1, 0), tile);
            bottomEntrances[j] = new Vector2(i, -height / 2 + y + 1);
            j++;
        }
    }

    internal void DrawEntrance(Tilemap tilemap, TileBase[] tiles)
    {
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 0), tiles[7]);
            }
        }
    }

    internal bool PlayerIsInside(Player player)
    {
        Vector3Int playerPos = new Vector3Int();
        playerPos.x = Mathf.RoundToInt(player.transform.position.x);
        playerPos.y = Mathf.RoundToInt(player.transform.position.y);

        for (int i = -width / 2 + x; i <= width / 2 + x; i++)
        {
            for (int j = -height / 2 + y; j <= height / 2 + y; j++)
            {
                Vector3Int pos = new Vector3Int(i, j, 0);
                if (playerPos.Equals(pos))
                {
                    return true;
                }
            }
        }

        return false;
    }
}