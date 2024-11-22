using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGeneratorScript : MonoBehaviour
{
    private Tilemap tilemap;
    public Tile floorTile, wallTile, crackedWallTile;

    // Start is called before the first frame update
    void Start()
    {
        //find tilemap component
        tilemap = GetComponent<Tilemap>();
        //tilemap.SetTile(new Vector3Int(-4,-5,0), wallTile);

        //generate first rectangle room shape
        GenerateRoom(new(0, 0), new(8, 10));
        //expand room by generating multiple overlapping rectangular room shapes
        for (int i = 0; i < 4; i++)
        {
            GenerateRoom(new(Random.Range(-4, 4), Random.Range(-5, 5)), new(Random.Range(6, 14), Random.Range(6, 14)));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateRoom(Vector2Int position, Vector2Int size)
    {
        Vector2Int halfSize = size / 2;
        for (int y = -halfSize.y; y < halfSize.y; y++)
        {
            for (int x = -halfSize.x; x < halfSize.x; x++)
            {
                Vector3Int tilePos = new(position.x + x, position.y + y, 0);
                if (tilemap.GetTile(tilePos) == floorTile) continue;
                if(x == -halfSize.x || x == halfSize.x - 1 || y == -halfSize.y || y == halfSize.y - 1)
                    tilemap.SetTile(tilePos, Random.Range(0, 5) == 0 ? crackedWallTile : wallTile);
                else
                    tilemap.SetTile(tilePos, floorTile);
            }
        }
    }
}
