using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        backgroundTilemap.SetTile(new Vector3Int(0, 0, 0), tiles[2]);
    }

}
