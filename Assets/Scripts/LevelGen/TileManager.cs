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
        DoorLeft,
        DoorRight,
        DoorVertical,
    }

    public class TileManager : MonoBehaviour
    {
        public GameObject tilePalettePrefab;
        public List<TileBase> tiles = new List<TileBase>();

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