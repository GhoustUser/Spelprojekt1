using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LevelGen
{
    public enum TileType
    {
        Any,
        Empty,
        Floor,
        Wall,
        Door,
        DoorLeft,
        DoorRight,
        DoorVertical,
    }
    public enum DoorState
    {
        Open,
        Opening,
        Closed
    }

    public enum DoorDirection
    {
        Left,
        Right,
        Vertical
    }

    public enum RoomType
    {
        Default,
        Start,
        End,
        Hallway,
        Arena1,
        Arena2,
        Arena3
    }

    public class TileManager : MonoBehaviour
    {
        public GameObject tilePalettePrefab;
        public List<TileBase> tiles = new List<TileBase>();

        public FloorTile floorTile;
        public WallTile wallTile;
        public VoidTile voidTile;
        public AirlockTile airlockTileClosed, airlockTileMidway, airlockTileOpen;

        public static readonly Vector2Int[] directions =
            { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        public static readonly Vector2Int[] directions8 =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, new(1, 1), new(1, -1), new(-1, -1),
            new(-1, 1)
        };
        public static bool IsDoor(TileType tileType)
        {
            return tileType == TileType.Door || tileType == TileType.DoorLeft || tileType == TileType.DoorRight || tileType == TileType.DoorVertical;
        }

        public void LoadTiles()
        {
            if (tilePalettePrefab == null)
            {
                Debug.LogError("Tile Palette prefab is not assigned!");
                return;
            }

            // Load the Tile Palette
            Tilemap tilemap = tilePalettePrefab.GetComponentInChildren<Tilemap>();

            if (tilemap == null)
            {
                Debug.LogError("No Tilemap found in the Tile Palette prefab!");
                return;
            }

            // Get all tiles from the Tilemap
            GetTilesFromTilemap(tilemap);

            // Example: Log the tiles
            foreach (var tile in tiles)
            {
                //Debug.Log("Tile found: " + tile.name);
            }
        }

        public void GetTilesFromTilemap(Tilemap tilemap)
        {
            // Iterate through all cells in the Tilemap's bounds
            BoundsInt bounds = tilemap.cellBounds;
            for (int y = bounds.xMax; y >= bounds.yMin; y--)
            {
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    Vector3Int cellPosition = new Vector3Int(x, y, 0);
                    TileBase tile = tilemap.GetTile(cellPosition);

                    if (tile != null && !tiles.Contains(tile))
                    {
                        tiles.Add(tile);
                    }
                }
            }
        }
    }
}