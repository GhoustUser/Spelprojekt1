using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGeneratorScript : MonoBehaviour
{
    private Tilemap tilemap;
    public Tile floorTile, wallTile, crackedWallTile;

    public static Dictionary<Vector2, Cell> cells = new Dictionary<Vector2, Cell>();

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

        GenerateGrid();
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

    private void GenerateGrid()
    {
        Vector3Int halfSize = tilemap.size / 2;
        for (int i = -tilemap.size.x; i < tilemap.size.x; i++)
        {
            for (int j = -tilemap.size.y; j < tilemap.size.y; j++)
            {
                if (tilemap.GetTile(new Vector3Int(i, j, 0)) == null) continue;

                cells.Add(new Vector2(i, j), 
                    new Cell(new Vector2(i, j), 
                    tilemap.GetTile(new Vector3Int(i, j, 0)) == floorTile));
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach (KeyValuePair<Vector2, Cell> pair in cells)
        {
            Gizmos.color = pair.Value.walkable ? Color.white : Color.green;

            Gizmos.DrawCube(pair.Key + (Vector2)transform.position + new Vector2(0.5f, 0.5f), new Vector3(1, 1));
        }
    }
}
