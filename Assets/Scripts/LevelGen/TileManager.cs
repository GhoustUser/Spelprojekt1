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

    public class TileManager : MonoBehaviour
    {
        /*
        public GameObject tilePalettePrefab;
        public List<TileBase> tiles = new List<TileBase>();
        */

        [Header("Default Tiles")]
        public FloorTile floorTile;
        public WallTile wallTile;
        public VoidTile voidTile;
        public AirlockTile airlockTileClosed, airlockTileMidway, airlockTileOpen;

        [Header("Lounge Tiles")] public FloorTile floorTile_lounge;
        public WallTile wallTile_lounge;

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

        public static List<TileBase> LoadTilesFromPalette(GameObject tilePalette)
        {
            if (tilePalette == null)
            {
                Debug.LogError("Invalid tile palette");
                return null;
            }

            // Load the Tile Palette
            Tilemap tilemap = tilePalette.GetComponentInChildren<Tilemap>();

            if (tilemap == null)
            {
                Debug.LogError("No Tilemap found in tile palette");
                return null;
            }

            // Get all tiles from the Tilemap
            List<TileBase> tiles = GetTilesFromTilemap(tilemap);

            // Example: Log the tiles
            foreach (var tile in tiles)
            {
                //Debug.Log("Tile found: " + tile.name);
            }

            return tiles;
        }

        private static List<TileBase> GetTilesFromTilemap(Tilemap tilemap)
        {
            List<TileBase> tiles = new List<TileBase>();
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

            return tiles;
        }
    }
}